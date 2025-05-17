using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Books;

public class AtivesBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapPatch("/actives", HandleAsync)
        .WithName("Ativa uma lista de livro")
        .WithSummary("Ativa uma lista de livro")
        .WithDescription("Ativa uma lista de livro")
        .WithOrder(6)
        .Produces<BaseResult>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromQuery(Name = "ids")] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return TypedResults.BadRequest(new BaseResult(false, "Nenhum ID foi fornecido."));
        }

        var result = await mediator.Send(new AtivesBookCommand(ids));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}