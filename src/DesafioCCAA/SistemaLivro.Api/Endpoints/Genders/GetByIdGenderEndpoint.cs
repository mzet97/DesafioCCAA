using SistemaLivro.Application.UseCases.Genders.Queries;
using SistemaLivro.Application.UseCases.Genders.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Genders;

public class GetByIdGenderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapGet("/{id}", HandleAsync)
        .WithName("Obtem gênero pelo id")
        .WithSummary("Obtem gênero pelo id")
        .WithDescription("Obtem gênero pelo id. Cache de 1 minutos")
        .WithOrder(2)
        .Produces<BaseResult<GenderViewModel>>()
        .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        var result = await mediator.Send(new GetByIdGenderQuery(id));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}