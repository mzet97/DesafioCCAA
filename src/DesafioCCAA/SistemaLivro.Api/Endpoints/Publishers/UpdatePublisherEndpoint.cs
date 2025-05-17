using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Publishers;

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
