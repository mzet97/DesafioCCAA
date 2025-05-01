using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using LinqKit;
using MediatR;
using System.Linq.Expressions;

namespace DesafioCCAA.Application.UseCases.Publishers.Queries.Handlers;

public class SearchPublisherQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<SearchPublisherQuery, BaseResultList<PublisherViewModel>>
{
    public async Task<BaseResultList<PublisherViewModel>> Handle(SearchPublisherQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Publisher, bool>>? filter = PredicateBuilder.New<Publisher>(true);
        Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>? ordeBy = null;

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

        filter = filter.And(x => x.IsDeleted == request.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Order))
        {
            switch (request.Order)
            {
                case "Id":
                    ordeBy = x => x.OrderBy(n => n.Id);
                    break;

                case "Name":
                    ordeBy = x => x.OrderBy(n => n.Name);
                    break;

                case "Description":
                    ordeBy = x => x.OrderBy(n => n.Description);
                    break;

                case "CreatedAt":
                    ordeBy = x => x.OrderBy(n => n.CreatedAt);
                    break;

                case "UpdatedAt":
                    ordeBy = x => x.OrderBy(n => n.UpdatedAt);
                    break;

                case "DeletedAt":
                    ordeBy = x => x.OrderBy(n => n.DeletedAt);
                    break;

                default:
                    ordeBy = x => x.OrderBy(n => n.Id);
                    break;
            }
        }

        var result = await unitOfWork.RepositoryFactory.PublisherRepository
          .SearchAsync(
              filter,
              ordeBy,
              "",
              request.PageSize,
              request.PageIndex);

        return new BaseResultList<PublisherViewModel>(
            result.Data.Select(x => PublisherViewModel.FromEntity(x)).ToList(), result.PagedResult);
    }
}
