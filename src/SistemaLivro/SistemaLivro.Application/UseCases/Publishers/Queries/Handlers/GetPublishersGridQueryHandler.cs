using Gridify;
using Gridify.EntityFramework;
using MediatR;
using SistemaLivro.Application.UseCases.Publishers.Queries;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Queries.Handlers;

public class GetPublishersGridQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetPublishersGridQuery, BaseResultList<PublisherViewModel>>
{
    public async Task<BaseResultList<PublisherViewModel>> Handle(GetPublishersGridQuery request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.RepositoryFactory.PublisherRepository;

        var query = repository.GetQueryable()
            .Where(p => !p.IsDeleted);

        // Aplica filtros, ordenação e paginação usando Gridify
        var gridifyMapper = new GridifyMapper<Publisher>()
            .AddMap("name", x => x.Name)
            .AddMap("description", x => x.Description)
            .AddMap("createdAt", x => x.CreatedAt)
            .AddMap("updatedAt", x => x.UpdatedAt);

        var pagedResult = await query.GridifyAsync(request, gridifyMapper);

        var publisherViewModels = pagedResult.Data.Select(PublisherViewModel.FromEntity).ToList();

        var totalCount = pagedResult.Count;
        var pageCount = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new BaseResultList<PublisherViewModel>(
            publisherViewModels,
            new PagedResult(
                request.Page,
                pageCount,
                request.PageSize,
                totalCount
            ),
            true,
            "Success"
        );
    }
}
