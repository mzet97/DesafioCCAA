using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Application.UseCases.Auth.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Auth;

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
