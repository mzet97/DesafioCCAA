using Gridify;
using MediatR;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Queries;

public class GetPublishersGridQuery : IGridifyQuery, IRequest<BaseResultList<PublisherViewModel>>
{
    public string? Filter { get; set; } = string.Empty;
    public string? OrderBy { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public GetPublishersGridQuery(string filter = "", string orderBy = "", int page = 1, int pageSize = 10)
    {
        Filter = filter;
        OrderBy = orderBy;
        Page = page;
        PageSize = pageSize;
    }

    public GetPublishersGridQuery()
    {
    }
}
