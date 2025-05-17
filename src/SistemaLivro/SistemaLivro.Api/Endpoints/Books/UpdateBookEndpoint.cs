using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;
using System.Security.Claims;

namespace SistemaLivro.Api.Endpoints.Books;

public class UpdateBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Atualiza um livro")
            .WithSummary("Atualiza um livro")
            .WithDescription("Atualiza um livro")
            .WithOrder(4)
            .Produces<BaseResult<BookViewModel>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        HttpContext httpContext,
        [FromRoute] Guid id,
        [FromBody] UpdateBookCommand command)
    {

        if (id != command.Id)
        {
            return TypedResults.BadRequest("Id da rota e Id do corpo da requisição não são iguais");
        }

        var user = httpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserUpdatedId = Guid.Parse(userId);
        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}
