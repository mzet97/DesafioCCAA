using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Shared.Responses;
using MediatR;
using SistemaLivro.Api.Common.Api;
using System.Security.Claims;

namespace SistemaLivro.Api.Endpoints.Books;

public class CreateBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapPost("/", HandleAsync)
        .WithName("Criar um nova livro")
        .WithSummary("Criar um nova livro")
        .WithDescription("Criar um nova livro")
        .WithOrder(1)
        .Produces<BaseResult<Guid>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        HttpContext httpContext,
        CreateBookCommand command)
    {
        var user = httpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserCreatedId = Guid.Parse(userId);
        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}