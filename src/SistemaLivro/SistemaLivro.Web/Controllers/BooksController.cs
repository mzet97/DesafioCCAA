using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Application.UseCases.Books.Queries;
using SistemaLivro.Application.UseCases.Genders.Queries;
using SistemaLivro.Application.UseCases.Publishers.Queries;
using SistemaLivro.Domain.Domains.Books.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace SistemaLivro.Web.Controllers;

[Authorize]
public class BooksController(
    ILogger<BooksController> logger,
    IMediator mediator) : Controller
{
    [HttpGet]
    public IActionResult Index()
       => View();

    [HttpGet]
    public async Task<IActionResult> LoadData(
        [FromQuery] int draw,
        [FromQuery] int start,
        [FromQuery] int length,
        [FromQuery(Name = "search[value]")] string? globalFilter,
        [FromQuery(Name = "order[0][column]")] int orderColumn,
        [FromQuery(Name = "order[0][dir]")] string orderDir,
        [FromQuery(Name = "columns[1][search][value]")] string idFilter,
        [FromQuery(Name = "columns[2][search][value]")] string titleFilter,
        [FromQuery(Name = "columns[3][search][value]")] string authorFilter,
        [FromQuery(Name = "columns[4][search][value]")] string synopsisFilter,
        [FromQuery(Name = "columns[5][search][value]")] string isbnFilter,
        [FromQuery(Name = "columns[6][search][value]")] string genderNameFilter,
        [FromQuery(Name = "columns[7][search][value]")] string publisherNameFilter,
        [FromQuery(Name = "columns[8][search][value]")] string createdAtFilter,
        [FromQuery(Name = "columns[9][search][value]")] string updatedAtFilter,
        [FromQuery(Name = "columns[10][search][value]")] string deletedAtFilter,
        [FromQuery(Name = "columns[11][search][value]")] string isDeletedFilter
    )
    {
        var cols = new[]
        {
            "Id", "Title", "Author",
            "Synopsis", "ISBN", "GenderName",
            "PublisherName", "CreatedAt", "UpdatedAt", 
            "DeletedAt", "IsDeleted"
        };

        var order = $"{cols[orderColumn - 1]} {orderDir}";

        Guid idVal =
            Guid.TryParse(idFilter, out var g) ? g : Guid.Empty;
        DateTime createdVal =
            DateTime.TryParse(createdAtFilter, out var dt1) ? dt1 : default;
        DateTime updatedVal =
            DateTime.TryParse(updatedAtFilter, out var dt2) ? dt2 : default;
        DateTime deletedVal =
            DateTime.TryParse(deletedAtFilter, out var dt3) ? dt3 : new DateTime();

        bool? isDeletedVal = isDeletedFilter switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };

        var query = new SearchBookQuery
        {
            PageIndex = start / length + 1,
            PageSize = length,
            Order = order,
            GlobalFilter = globalFilter,
            Id = idVal,
            Title = titleFilter,
            Author = authorFilter,
            Synopsis = synopsisFilter,
            ISBN = isbnFilter,
            GenderName = genderNameFilter,
            PublisherName = publisherNameFilter,
            CreatedAt = createdVal,
            UpdatedAt = updatedVal,
            DeletedAt = deletedVal,
            IsDeleted = isDeletedVal
        };

        var result = await mediator.Send(query);

        return Json(new
        {
            draw,
            recordsTotal = result.PagedResult.RowCount,
            recordsFiltered = result.PagedResult.RowCount,
            data = result.Data
        });
    }

    [HttpPost]
    public async Task<IActionResult> BulkActivate([FromBody] Guid[] ids)
    {
        await mediator.Send(new AtivesBookCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDeactivate([FromBody] Guid[] ids)
    {
        await mediator.Send(new DisablesBookCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDelete([FromBody] Guid[] ids)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await mediator.Send(new DeletesBookCommand(ids.ToList(), Guid.Parse(userId)));
        return Ok();
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCreateViewBags();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateBookCommand command,
        IFormFile coverFile)
    {
        try
        {
            if (coverFile is null || coverFile.Length == 0)
            {
                ModelState.AddModelError(nameof(coverFile), 
                    "A capa do livro é obrigatória.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBags();
                return View(command);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            command.UserCreatedId = userId;

            var createResult = await mediator.Send(command);
            if (createResult == null || !createResult.Success)
            {
                ModelState.AddModelError(
                    string.Empty, 
                    createResult?.Message ?? 
                    "Erro ao criar o livro.");
                await PopulateCreateViewBags();
                return View(command);
            }

            var newBookId = createResult.Data;
            if (coverFile != null && coverFile.Length > 0)
            {
                await using var stream = coverFile.OpenReadStream();
                await mediator.Send(new UploadImageCommand(
                    newBookId,
                    userId,
                    stream,
                    coverFile.FileName
                ));
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);

            ModelState.AddModelError(
              string.Empty,
              "Ocorreu um erro ao criar a gênero: " + ex.Message
            );

            return View();
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        await PopulateCreateViewBags();

        var result = await mediator.Send(new GetByIdBookQuery(id));
        if (result is null) return NotFound();

        var command = new UpdateBookCommand
        {
            Id = result.Data.Id,
            Title = result.Data.Title,
            Author = result.Data.Author,
            Synopsis = result.Data.Synopsis,
            ISBN = result.Data.ISBN,
            GenderId = result.Data.GenderId,
            PublisherId = result.Data.PublisherId,
            UserUpdatedId = result.Data.UserUpdatedId ?? Guid.Empty
        };

        try
        {
            var base64 = await mediator.Send(new GetFileBase64Query(id));
            ViewBag.CoverBase64 = base64.Data;
        }
        catch (FileNotFoundException)
        {
            ViewBag.CoverBase64 = null;
        }

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        UpdateBookCommand command,
        IFormFile? coverFile)
    {
        try
        {
            if (coverFile != null && coverFile.Length == 0)
                ModelState.AddModelError(nameof(coverFile), "Arquivo inválido.");

            if (!ModelState.IsValid)
            {
                try { ViewBag.CoverBase64 = await mediator.Send(new GetFileBase64Query(id)); }
                catch { ViewBag.CoverBase64 = null; }
                return View(command);
            }

            command.UserUpdatedId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var updateResult = await mediator.Send(command);
            if (updateResult == null || !updateResult.Success)
            {
                ModelState.AddModelError(string.Empty, updateResult?.Message ?? "Erro ao editar o livro");
                try { ViewBag.CoverBase64 = await mediator.Send(new GetFileBase64Query(id)); }
                catch { ViewBag.CoverBase64 = null; }
                return View(command);
            }

            if (coverFile != null && coverFile.Length > 0)
            {
                await using var stream = coverFile.OpenReadStream();
                await mediator.Send(new UploadImageCommand(
                    id,
                command.UserUpdatedId,
                    stream,
                    coverFile.FileName
                ));
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);

            ModelState.AddModelError(
              string.Empty,
              "Ocorreu um erro ao editar a gênero: " + ex.Message
            );

            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Report()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var resultPdf = await mediator.Send(new GenerateBookReportQuery(userId));

        if(resultPdf is null || resultPdf.Data is null)
        {
            ModelState.AddModelError(string.Empty, resultPdf?.Message ?? "Erro ao gerar PDF");
            return View();
        }

        return File(resultPdf.Data, "application/pdf",
                    $"books-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
    }

    private async Task PopulateCreateViewBags()
    {
        var publishers = await mediator.Send(new SearchPublisherQuery { PageIndex = 1, PageSize = 200 });
        var genders = await mediator.Send(new SearchGenderQuery { PageIndex = 1, PageSize = 200 });

        ViewBag.Publishers = new SelectList(publishers.Data, nameof(Publisher.Id), nameof(Publisher.Name));
        ViewBag.Genders = new SelectList(genders.Data, nameof(Gender.Id), nameof(Gender.Name));
    }
}