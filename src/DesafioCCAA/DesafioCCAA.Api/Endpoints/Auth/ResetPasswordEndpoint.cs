using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Auth;

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