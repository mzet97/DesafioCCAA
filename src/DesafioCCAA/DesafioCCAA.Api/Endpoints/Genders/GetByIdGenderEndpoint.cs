using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Genders.Queries;
using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Genders;

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