using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Auth;

public class ForgotPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/forgot-password", HandleAsync)
            .WithName("RequestPasswordReset")
            .WithSummary("Solicita link de redefinição de senha")
            .WithDescription("Gera um token de reset e envia por e-mail")
            .WithOrder(4)
            .Accepts<RequestPasswordResetCommand>("application/json")
            .Produces<BaseResult<bool>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [FromBody] RequestPasswordResetCommand cmd)
    {
        var result = await mediator.Send(cmd);
        return result.Success
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}
