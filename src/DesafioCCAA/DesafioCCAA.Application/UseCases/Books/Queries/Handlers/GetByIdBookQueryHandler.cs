using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Queries.Handlers;

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
