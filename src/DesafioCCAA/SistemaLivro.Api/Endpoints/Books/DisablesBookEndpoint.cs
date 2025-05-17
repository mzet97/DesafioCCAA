using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Books;

public class DisablesBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapPatch("/desables", HandleAsync)
        .WithName("Desativa uma lista de livro")
        .WithSummary("Desativa uma lista de livro")
        .WithDescription("Desativa uma lista de livro")
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

        var result = await mediator.Send(new DisablesBookCommand(ids));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}