using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Auth;

public class ResetPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/reset-password", HandleAsync)
            .WithName("ResetPassword")
            .WithSummary("Aplica nova senha ao usuário")
            .WithDescription("Valida token e redefine a senha")
            .WithOrder(5)
            .Accepts<ResetPasswordCommand>("application/json")
            .Produces<BaseResult<bool>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromBody] ResetPasswordCommand cmd)
    {
        var result = await mediator.Send(cmd);
        return result.Success
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}