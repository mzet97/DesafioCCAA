using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using LinqKit;
using MediatR;
using System.Linq.Expressions;

namespace DesafioCCAA.Application.UseCases.Books.Queries.Handlers;

public class SearchBookQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<SearchBookQuery, BaseResultList<BookViewModel>>
{
    public async Task<BaseResultList<BookViewModel>> Handle(SearchBookQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Book, bool>>? filter = PredicateBuilder.New<Book>(true);
        Func<IQueryable<Book>, IOrderedQueryable<Book>>? ordeBy = null;

        if (!string.IsNullOrWhiteSpace(request.GlobalFilter))
        {
            filter = filter.And(x =>
                x.Title.Contains(request.Title) ||
                x.Author.Contains(request.Author) ||
                x.Synopsis.Contains(request.Synopsis) ||
                x.ISBN.Contains(request.ISBN) ||
                x.Gender.Name.Contains(request.GenderName) ||
                x.Publisher.Name.Contains(request.PublisherName)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            filter = filter.And(x => x.Title.Contains(request.Title));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            filter = filter.And(x => x.Author.Contains(request.Author));
        }

        if (!string.IsNullOrWhiteSpace(request.Synopsis))
        {
            filter = filter.And(x => x.Synopsis.Contains(request.Synopsis));
        }

        if (!string.IsNullOrWhiteSpace(request.ISBN))
        {
            filter = filter.And(x => x.ISBN.Contains(request.ISBN));
        }

        if (!string.IsNullOrWhiteSpace(request.GenderName))
        {
            filter = filter.And(x => x.Gender.Name.Contains(request.GenderName));
        }

        if (!string.IsNullOrWhiteSpace(request.PublisherName))
        {
            filter = filter.And(x => x.Publisher.Name.Contains(request.PublisherName));
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

        if (request.IsDeleted is not null)
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

                case "Title asc":
                    ordeBy = x => x.OrderBy(n => n.Title);
                    break;

                case "Title desc":
                    ordeBy = x => x.OrderByDescending(n => n.Title);
                    break;

                case "Author asc":
                    ordeBy = x => x.OrderBy(n => n.Author);
                    break;

                case "Author desc":
                    ordeBy = x => x.OrderByDescending(n => n.Author);
                    break;

                case "Synopsis asc":
                    ordeBy = x => x.OrderBy(n => n.Synopsis);
                    break;

                case "Synopsis desc":
                    ordeBy = x => x.OrderByDescending(n => n.Synopsis);
                    break;

                case "ISBN asc":
                    ordeBy = x => x.OrderBy(n => n.ISBN);
                    break;

                case "ISBN desc":
                    ordeBy = x => x.OrderByDescending(n => n.ISBN);
                    break;

                case "Publisher Name asc":
                    ordeBy = x => x.OrderBy(n => n.Publisher.Name);
                    break;

                case "Publisher Name desc":
                    ordeBy = x => x.OrderByDescending(n => n.Publisher.Name);
                    break;

                case "Gender Name asc":
                    ordeBy = x => x.OrderBy(n => n.Gender.Name);
                    break;

                case "Gender Name desc":
                    ordeBy = x => x.OrderByDescending(n => n.Gender.Name);
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

        var result = await unitOfWork.RepositoryFactory.BookRepository
          .SearchAsync(
              filter,
              ordeBy,
              "Gender,Publisher",
              request.PageSize,
              request.PageIndex);

        return new BaseResultList<BookViewModel>(
            result.Data.Select(x => BookViewModel.FromEntity(x)).ToList(), result.PagedResult);
    }
}
