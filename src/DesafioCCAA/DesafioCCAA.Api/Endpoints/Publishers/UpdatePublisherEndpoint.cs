using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Publishers.Commands;
using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Publishers;

public class UpdatePublisherEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Atualiza um editora")
            .WithSummary("Atualiza um editora")
            .WithDescription("Atualiza um editora")
            .WithOrder(4)
            .Produces<BaseResult<PublisherViewModel>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromRoute] Guid id,
        [FromBody] UpdatePublisherCommand command)
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
