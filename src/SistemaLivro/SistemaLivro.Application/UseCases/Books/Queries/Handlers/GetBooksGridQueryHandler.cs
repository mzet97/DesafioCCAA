using Gridify;
using Gridify.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Queries.Handlers;

public class GetBooksGridQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetBooksGridQuery, BaseResultList<BookViewModel>>
{
    public async Task<BaseResultList<BookViewModel>> Handle(GetBooksGridQuery request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.RepositoryFactory.BookRepository;

        var query = repository.GetQueryable()
            .Include(b => b.Gender)
            .Include(b => b.Publisher)
            .Include(b => b.CoverImage)
            .Include(b => b.UserCreated)
            .Include(b => b.UserUpdated)
            .Where(b => !b.IsDeleted);

        // Aplica filtros, ordenação e paginação usando Gridify
        var gridifyMapper = new GridifyMapper<Book>()
            .AddMap("title", x => x.Title)
            .AddMap("author", x => x.Author)
            .AddMap("isbn", x => x.ISBN)
            .AddMap("synopsis", x => x.Synopsis)
            .AddMap("genderName", x => x.Gender.Name)
            .AddMap("publisherName", x => x.Publisher.Name)
            .AddMap("createdAt", x => x.CreatedAt)
            .AddMap("updatedAt", x => x.UpdatedAt);

        var pagedResult = await query.GridifyAsync(request.GridifyQuery, gridifyMapper);

        var bookViewModels = pagedResult.Data.Select(BookViewModel.FromEntity).ToList();

        var totalCount = pagedResult.Count;
        var pageCount = (int)Math.Ceiling((double)totalCount / request.GridifyQuery.PageSize);

        return new BaseResultList<BookViewModel>(
            bookViewModels,
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
