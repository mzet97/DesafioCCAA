using SistemaLivro.Application.UseCases.Books.Commands;
using MediatR;
using SistemaLivro.Api.Common.Api;
using System.Security.Claims;

namespace SistemaLivro.Api.Endpoints.Books;

public class UploadImageEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id}/image", HandleAsync)
              .Accepts<IFormFile>("multipart/form-data")
              .DisableAntiforgery()
              .WithName("UploadImage")
              .WithSummary("Faz upload de uma imagem")
              .WithDescription("Recebe imagem via multipart/form-data e retorna o nome do arquivo salvo")
              .WithOrder(8)
              .Produces<string>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        HttpContext httpContext,
        Guid id,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return TypedResults.BadRequest("Arquivo inválido.");

        try
        {
            var user = httpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            await using var stream = file.OpenReadStream();
            var fileName = await mediator.Send(new UploadImageCommand(
                id,
                Guid.Parse(userId),
                stream, 
                file.FileName));
            return TypedResults.Ok(fileName);
        }
        catch (InvalidDataException ide)
        {
            return TypedResults.BadRequest(ide.Message);
        }
    }
}