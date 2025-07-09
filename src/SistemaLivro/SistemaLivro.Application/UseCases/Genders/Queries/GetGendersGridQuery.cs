using Gridify;
using MediatR;
using SistemaLivro.Application.UseCases.Genders.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Queries;

public class GetGendersGridQuery : IRequest<BaseResultList<GenderViewModel>>
{
    public GridifyQuery GridifyQuery { get; set; } = new();

    public GetGendersGridQuery()
    {
        GridifyQuery = new GridifyQuery
        {
            Page = 1,
            PageSize = 10
        };
    }

    public GetGendersGridQuery(GridifyQuery gridifyQuery)
    {
        GridifyQuery = gridifyQuery;
    }
}
