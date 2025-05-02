using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Api.Endpoints.Genders;

public class CreateGenderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapPost("/", HandleAsync)
        .WithName("Criar um nova gênero")
        .WithSummary("Criar um nova gênero")
        .WithDescription("Criar um nova gênero")
        .WithOrder(1)
        .Produces<BaseResult<Guid>>();

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CreateGenderCommand command)
    {

        var result = await mediator.Send(command);

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}