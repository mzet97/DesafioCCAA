using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Queries.Handlers;

public class GetByIdBookQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetByIdBookQuery, BaseResult<BookViewModel>>
{
    public async Task<BaseResult<BookViewModel>> Handle(GetByIdBookQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var Book = await unitOfWork
            .RepositoryFactory
            .BookRepository
            .GetByIdAsync(request.Id);

        if (Book is null)
            throw new NotFoundException("Not found");

        return new BaseResult<BookViewModel>(
            BookViewModel
            .FromEntity(Book));
    }
}
