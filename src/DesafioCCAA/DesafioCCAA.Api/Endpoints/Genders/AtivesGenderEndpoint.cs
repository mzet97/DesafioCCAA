using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Genders;

public class AtivesGenderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapPatch("/actives", HandleAsync)
        .WithName("Ativa uma lista de gênero")
        .WithSummary("Ativa uma lista de gênero")
        .WithDescription("Ativa uma lista de gênero")
        .WithOrder(7)
        .Produces<BaseResult>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromQuery(Name = "ids")] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return TypedResults.BadRequest(new BaseResult(false, "Nenhum ID foi fornecido."));
        }

        var result = await mediator.Send(new AtivesGenderCommand(ids));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}