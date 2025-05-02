using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Genders;

public class UpdateGenderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Atualiza um gênero")
            .WithSummary("Atualiza um gênero")
            .WithDescription("Atualiza um gênero")
            .WithOrder(4)
            .Produces<BaseResult<GenderViewModel>>();

    private static async Task<IResult> HandleAsync(
    IMediator mediator,
        [FromRoute] Guid id,
        [FromBody] UpdateGenderCommand command)
    {

        if (id != command.Id)
        {
            return TypedResults.BadRequest("Id da rota e Id do corpo da requisição não são iguais");
        }

        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}
