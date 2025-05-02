using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Application.UseCases.Publishers.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Genders;

public class DeletesGenderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapDelete("/", HandleAsync)
        .WithName("Deleta uma lista de gênero")
        .WithSummary("Deleta uma lista de gênero")
        .WithDescription("Deleta uma lista de gênero")
        .WithOrder(5)
        .Produces<BaseResult>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromQuery(Name = "ids")] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return TypedResults.BadRequest(new BaseResult(false, "Nenhum ID foi fornecido."));
        }

        var result = await mediator.Send(new DeletesGenderCommand(ids));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}