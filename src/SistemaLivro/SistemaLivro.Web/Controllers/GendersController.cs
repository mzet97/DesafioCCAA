using SistemaLivro.Application.UseCases.Genders.Commands;
using SistemaLivro.Application.UseCases.Genders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaLivro.Web.Controllers;

[Authorize]
public class GendersController(
    ILogger<GendersController> logger,
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
        [FromQuery(Name = "columns[2][search][value]")] string nameFilter,
        [FromQuery(Name = "columns[3][search][value]")] string descFilter,
        [FromQuery(Name = "columns[4][search][value]")] string createdAtFilter,
        [FromQuery(Name = "columns[5][search][value]")] string updatedAtFilter,
        [FromQuery(Name = "columns[6][search][value]")] string deletedAtFilter,
        [FromQuery(Name = "columns[7][search][value]")] string isDeletedFilter
    )
    {
        var cols = new[]
        {
            "Id", "Name", "Description",
            "CreatedAt", "UpdatedAt", "DeletedAt", "IsDeleted"
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

        var query = new SearchGenderQuery
        {
            PageIndex = start / length + 1,
            PageSize = length,
            Order = order,
            GlobalFilter = globalFilter,
            Id = idVal,
            Name = nameFilter,
            Description = descFilter,
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
        await mediator.Send(new AtivesGenderCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDeactivate([FromBody] Guid[] ids)
    {
        await mediator.Send(new DisablesGenderCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDelete([FromBody] Guid[] ids)
    {
        await mediator.Send(new DeletesGenderCommand(ids.ToList()));
        return Ok();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGenderCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            if (result is null)
                throw new Exception("Erro ao criar a gênero");

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message
                );
                return View(command);
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
        var result = await mediator.Send(new GetByIdGenderQuery(id));

        if (result is null)
        {
            return NotFound();
        }

        var command = new UpdateGenderCommand()
        {
            Id = result.Data.Id,
            Name = result.Data.Name,
            Description = result.Data.Description
        };

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateGenderCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            if (result is null)
                throw new Exception("Erro ao editar a gênero");

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message
                );
                return View(command);
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
}