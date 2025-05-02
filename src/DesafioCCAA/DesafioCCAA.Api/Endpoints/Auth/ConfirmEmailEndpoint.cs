using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints.Auth;

public class ConfirmEmailEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/confirm-email", HandleAsync)
            .WithName("Comfirma email")
            .WithSummary("Comfirma email")
            .WithDescription("Comfirma email")
            .WithOrder(3)
            .Produces<BaseResult<bool>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        [AsParameters] ConfirmEmailQuery search)
    {

        var command = new ConfirmEmailCommand
        {
            UserId = search.userId,
            Token = search.token
        };

        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}

public class ConfirmEmailQuery
{
    [FromQuery] public string? userId { get; set; }
    [FromQuery] public string? token { get; set; }
}