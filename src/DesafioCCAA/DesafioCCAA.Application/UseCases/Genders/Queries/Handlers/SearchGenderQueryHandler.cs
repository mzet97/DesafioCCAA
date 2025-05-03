using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using LinqKit;
using MediatR;
using System.Linq.Expressions;

namespace DesafioCCAA.Application.UseCases.Genders.Queries.Handlers;

public class SearchGenderQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<SearchGenderQuery, BaseResultList<GenderViewModel>>
{
    public async Task<BaseResultList<GenderViewModel>> Handle(SearchGenderQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Gender, bool>>? filter = PredicateBuilder.New<Gender>(true);
        Func<IQueryable<Gender>, IOrderedQueryable<Gender>>? ordeBy = null;

        if (!string.IsNullOrWhiteSpace(request.GlobalFilter))
        {
            filter = filter.And(x =>
                x.Name.Contains(request.Name) ||
                x.Description.Contains(request.Description)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filter = filter.And(x => x.Name.Contains(request.Name));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            filter = filter.And(x => x.Description.Contains(request.Description));
        }

        if (request.Id != Guid.Empty)
        {
            filter = filter.And(x => x.Id == request.Id);
        }

        if (request.CreatedAt != default)
        {
            filter = filter.And(x => x.CreatedAt == request.CreatedAt);
        }

        if (request.UpdatedAt != default)
        {
            filter = filter.And(x => x.UpdatedAt == request.UpdatedAt);
        }

        if (request.DeletedAt != new DateTime())
        {
            filter = filter.And(x => x.DeletedAt == request.DeletedAt);
        }

        if(request.IsDeleted is not null)
            filter = filter.And(x => x.IsDeleted == request.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Order))
        {
            switch (request.Order)
            {
                case "Id asc":
                    ordeBy = x => x.OrderBy(n => n.Id);
                    break;

                case "Id desc":
                    ordeBy = x => x.OrderByDescending(n => n.Id);
                    break;

                case "Name asc":
                    ordeBy = x => x.OrderBy(n => n.Name);
                    break;

                case "Name desc":
                    ordeBy = x => x.OrderByDescending(n => n.Name);
                    break;

                case "Description asc":
                    ordeBy = x => x.OrderBy(n => n.Description);
                    break;

                case "Description desc":
                    ordeBy = x => x.OrderByDescending(n => n.Description);
                    break;

                case "CreatedAt asc":
                    ordeBy = x => x.OrderBy(n => n.CreatedAt);
                    break;

                case "CreatedAt desc":
                    ordeBy = x => x.OrderByDescending(n => n.CreatedAt);
                    break;

                case "UpdatedAt asc":
                    ordeBy = x => x.OrderBy(n => n.UpdatedAt);
                    break;

                case "UpdatedAt desc":
                    ordeBy = x => x.OrderByDescending(n => n.UpdatedAt);
                    break;

                case "DeletedAt asc":
                    ordeBy = x => x.OrderBy(n => n.DeletedAt);
                    break;

                case "DeletedAt desc":
                    ordeBy = x => x.OrderBy(n => n.DeletedAt);
                    break;

                default:
                    ordeBy = x => x.OrderBy(n => n.Id);
                    break;
            }
        }

        var result = await unitOfWork.RepositoryFactory.GenderRepository
          .SearchAsync(
              filter,
              ordeBy,
              "",
              request.PageSize,
              request.PageIndex);

        return new BaseResultList<GenderViewModel>(
            result.Data.Select(x => GenderViewModel.FromEntity(x)).ToList(), result.PagedResult);
    }
}
