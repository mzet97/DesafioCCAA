using Gridify;
using Gridify.EntityFramework;
using MediatR;
using SistemaLivro.Application.UseCases.Genders.Queries;
using SistemaLivro.Application.UseCases.Genders.ViewModels;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Queries.Handlers;

public class GetGendersGridQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetGendersGridQuery, BaseResultList<GenderViewModel>>
{
    public async Task<BaseResultList<GenderViewModel>> Handle(GetGendersGridQuery request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.RepositoryFactory.GenderRepository;

        var query = repository.GetQueryable()
            .Where(g => !g.IsDeleted);

        // Aplica filtros, ordenação e paginação usando Gridify
        var gridifyMapper = new GridifyMapper<Gender>()
            .AddMap("name", x => x.Name)
            .AddMap("description", x => x.Description)
            .AddMap("createdAt", x => x.CreatedAt)
            .AddMap("updatedAt", x => x.UpdatedAt);

        var pagedResult = await query.GridifyAsync(request.GridifyQuery, gridifyMapper);

        var genderViewModels = pagedResult.Data.Select(GenderViewModel.FromEntity).ToList();

        var totalCount = pagedResult.Count;
        var pageCount = (int)Math.Ceiling((double)totalCount / request.GridifyQuery.PageSize);

        return new BaseResultList<GenderViewModel>(
            genderViewModels,
            new PagedResult(
                request.GridifyQuery.Page,
                pageCount,
                request.GridifyQuery.PageSize,
                totalCount
            ),
            true,
            "Success"
        );
    }
}
