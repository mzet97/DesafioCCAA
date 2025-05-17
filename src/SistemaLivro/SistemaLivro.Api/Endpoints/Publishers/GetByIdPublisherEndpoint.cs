using SistemaLivro.Application.UseCases.Publishers.Queries;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Publishers;

public class GetByIdPublisherEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapGet("/{id}", HandleAsync)
        .WithName("Obtem editora pelo id")
        .WithSummary("Obtem editora pelo id")
        .WithDescription("Obtem editora pelo id. Cache de 1 minutos")
        .WithOrder(2)
        .Produces<BaseResult<PublisherViewModel>>()
        .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        var result = await mediator.Send(new GetByIdPublisherQuery(id));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}