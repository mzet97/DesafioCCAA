using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Books.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DesafioCCAA.Api.Endpoints.Books;

public class DeletesBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapDelete("/", HandleAsync)
        .WithName("Deleta uma lista de livro")
        .WithSummary("Deleta uma lista de livro")
        .WithDescription("Deleta uma lista de livro")
        .WithOrder(5)
        .Produces<BaseResult>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
         HttpContext httpContext,
        [FromQuery(Name = "ids")] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return TypedResults.BadRequest(new BaseResult(false, "Nenhum ID foi fornecido."));
        }

        var user = httpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new DeletesBookCommand(ids, Guid.Parse(userId)));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}