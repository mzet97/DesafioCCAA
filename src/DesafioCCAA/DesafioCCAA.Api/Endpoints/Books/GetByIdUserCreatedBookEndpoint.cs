using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Books.Queries;
using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Books;

public class GetByIdUserCreatedBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapGet("/users/{id}", HandleAsync)
        .WithName("Obtem livros pelo id do usario que criou")
        .WithSummary("Obtem livros pelo id do usario que criou")
        .WithDescription("Obtem livros pelo id do usario que criou. Cache de 1 minutos")
        .WithOrder(11)
        .Produces<BaseResult<BookViewModel>>()
        .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        var result = await mediator.Send(new GetByIdUserCreatedBook(id));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}
