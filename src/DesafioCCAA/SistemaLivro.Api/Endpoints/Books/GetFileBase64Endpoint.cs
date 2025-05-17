using SistemaLivro.Application.UseCases.Books.Commands;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Books;

public class GetFileBase64Endpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/image/{id}/base64", HandleAsync)
              .WithName("GetFileBase64")
              .WithSummary("Obtém o arquivo em Base64")
              .WithDescription("Retorna a string Base64 do arquivo especificado")
              .WithOrder(9)
              .Produces<string>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status404NotFound)
              .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        try
        {
            
            var base64 = await mediator.Send(new GetFileBase64Query(id));
            return TypedResults.Ok(base64);
        }
        catch (FileNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
