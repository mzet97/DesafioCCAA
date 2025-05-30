﻿using SistemaLivro.Application.UseCases.Books.Queries;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;
using MediatR;
using SistemaLivro.Api.Common.Api;

namespace SistemaLivro.Api.Endpoints.Books;

public class GetByIdBookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    => app.MapGet("/{id}", HandleAsync)
        .WithName("Obtem livro pelo id")
        .WithSummary("Obtem livro pelo id")
        .WithDescription("Obtem livro pelo id. Cache de 1 minutos")
        .WithOrder(2)
        .Produces<BaseResult<BookViewModel>>()
        .CacheOutput("Short");

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        Guid id)
    {
        var result = await mediator.Send(new GetByIdBookQuery(id));

        if (result.Success)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.BadRequest(result);
    }
}
