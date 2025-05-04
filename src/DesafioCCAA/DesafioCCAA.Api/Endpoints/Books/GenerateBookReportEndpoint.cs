using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Books.Queries;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Books;


public class GenerateBookReportEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/report/{idUser:guid}", HandleAsync)
           .WithName("DownloadBookReportByUserId")
           .WithSummary("Gera e devolve o relatório de livros em PDF para download")
           .WithDescription("Retorna um arquivo PDF com os livros criados pelo usuário informado. Cache de 1 minuto.")
           .WithOrder(12)
           .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
           .Produces<BaseResult<byte[]>>(StatusCodes.Status400BadRequest)
           .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(IMediator mediator, Guid idUser)
    {
        var result = await mediator.Send(new GenerateBookReportQuery(idUser));

        if (result?.Success is true && result.Data is { Length: > 0 })
        {
            var fileName = $"books-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";

            return Results.File(
                fileContents: result.Data,
                contentType: "application/pdf",
                fileDownloadName: fileName);
        }

        return Results.BadRequest(result ?? new BaseResult<byte[]>(null, false, "Erro desconhecido"));
    }
}