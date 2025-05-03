using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Application.UseCases.Genders.Queries;
using DesafioCCAA.Application.UseCases.Publishers.Commands;
using DesafioCCAA.Application.UseCases.Publishers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Web.Controllers;

[Authorize]
public class PublishersController(
    ILogger<PublishersController> logger,
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

        var query = new SearchPublisherQuery
        {
            PageIndex = (start / length) + 1,
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
        await mediator.Send(new AtivesPublisherCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDeactivate([FromBody] Guid[] ids)
    {
        await mediator.Send(new DisablesPublisherCommand(ids.ToList()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> BulkDelete([FromBody] Guid[] ids)
    {
        await mediator.Send(new DeletesPublisherCommand(ids.ToList()));
        return Ok();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePublisherCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);

            return View();
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await mediator.Send(new GetByIdPublisherQuery(id));

        if (result is null)
        {
            return NotFound();
        }

        var command = new UpdatePublisherCommand()
        {
            Id = result.Data.Id,
            Name = result.Data.Name,
            Description = result.Data.Description
        };

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdatePublisherCommand command)
    {
        try
        {
            var result = await mediator.Send(command);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);

            return View();
        }
    }
}
