using Gridify;
using MediatR;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Queries;

public class GetBooksGridQuery : IRequest<BaseResultList<BookViewModel>>
{
    public GridifyQuery GridifyQuery { get; set; } = new();

    public GetBooksGridQuery()
    {
        GridifyQuery = new GridifyQuery
        {
            Page = 1,
            PageSize = 10
        };
    }

    public GetBooksGridQuery(GridifyQuery gridifyQuery)
    {
        GridifyQuery = gridifyQuery;
    }
}
