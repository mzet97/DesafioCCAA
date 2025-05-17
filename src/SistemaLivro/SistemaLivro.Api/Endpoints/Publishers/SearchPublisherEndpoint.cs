using SistemaLivro.Application.UseCases.Publishers.Queries;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Publishers;

public class SearchPublisherEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
       => app.MapGet("/", HandleAsync)
           .WithName("Busca editora")
           .WithSummary("Busca editora")
           .WithDescription("Busca editora Cache de 1 minutos.")
           .WithOrder(3)
           .Produces<BaseResultList<PublisherViewModel>>()
           .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [AsParameters] SearchOrder search)
    {
        var query = new SearchPublisherQuery
        {
            Description = search.Description ?? "",
            Name = search.Name ?? "",
            GlobalFilter = search.GlobalFilter ?? "",
            Id = search.Id ?? Guid.Empty,
            CreatedAt = search.CreatedAt ?? default,
            UpdatedAt = search.UpdatedAt ?? default,
            DeletedAt = search.DeletedAt ?? default,
            IsDeleted = search.IsDeleted ?? default,
            Order = search.Order ?? "",
            PageIndex = search.PageIndex ?? 1,
            PageSize = search.PageSize ?? 10,
        };

        var result = await mediator.Send(query);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}

public class SearchOrder
{
    [FromQuery] public string? Description { get; set; }
    [FromQuery] public string? Name { get; set; }
    [FromQuery] public string? GlobalFilter { get; set; }
    [FromQuery] public Guid? Id { get; set; }
    [FromQuery] public DateTime? DateOrder { get; set; }
    [FromQuery] public DateTime? CreatedAt { get; set; }
    [FromQuery] public DateTime? UpdatedAt { get; set; }
    [FromQuery] public DateTime? DeletedAt { get; set; }
    [FromQuery] public bool? IsDeleted { get; set; } = null;
    [FromQuery] public string? Order { get; set; }
    [FromQuery] public bool? Include { get; set; }
    [FromQuery] public int? PageIndex { get; set; } = 1;
    [FromQuery] public int? PageSize { get; set; } = 10;
}