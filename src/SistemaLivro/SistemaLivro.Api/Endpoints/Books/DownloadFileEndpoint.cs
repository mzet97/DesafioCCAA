using SistemaLivro.Application.UseCases.Books.Commands;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Books;

public class DownloadFileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/image/{id}", HandleAsync)
              .WithName("DownloadFile")
              .WithSummary("Baixa o arquivo")
              .WithDescription("Retorna o stream do arquivo para download")
              .WithOrder(10)
              .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
              .Produces(StatusCodes.Status404NotFound)
              .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        try
        {
            var stream = await mediator.Send(new DownloadFileQuery(id));
            return TypedResults.File(stream, "application/octet-stream",
                 id + "-" + DateTime.Now.ToString("dd-MM-yyyy"));
        }
        catch (FileNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
