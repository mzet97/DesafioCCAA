using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Auth.Commands;
using DesafioCCAA.Application.UseCases.Auth.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Auth;

public class RegisterUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
         => app.MapPost("/register", HandleAsync)
             .WithName("Criar um novo usuario")
             .WithSummary("Criar um novo usuario")
             .WithDescription("Criar um novo usuario")
             .WithOrder(2)
             .Produces<BaseResult<LoginResponseViewModel?>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        RegisterUserCommand command)
    {

        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}
