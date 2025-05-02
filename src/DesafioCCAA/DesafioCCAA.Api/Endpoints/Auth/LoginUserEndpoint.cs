using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Application.UseCases.Auth.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Auth;

public class LoginUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/login", HandleAsync)
            .WithName("Faz o login")
            .WithSummary("Faz o login")
            .WithDescription("Faz o login")
            .WithOrder(1)
            .Produces<BaseResult<LoginResponseViewModel?>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        LoginUserCommand command)
    {

        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}