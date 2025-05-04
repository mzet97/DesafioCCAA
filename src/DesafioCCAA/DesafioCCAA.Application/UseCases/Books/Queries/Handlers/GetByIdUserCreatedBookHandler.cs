using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Queries.Handlers;

public class GetByIdUserCreatedBookHandler(
    IUnitOfWork unitOfWork,
    IApplicationUserService applicationUserService,
    ILogger<GetByIdUserCreatedBookHandler> logger
    ) :
    IRequestHandler<GetByIdUserCreatedBook, BaseResultList<BookViewModel>>
{
    public async Task<BaseResultList<BookViewModel>> Handle(
        GetByIdUserCreatedBook request,
        CancellationToken cancellationToken)
    {

        var user = await applicationUserService.FindByIdAsync(request.Id);

        if (user is null)
        {
            logger.LogError("User not found");
            throw new NotFoundException("Usuário não encontrado");
        }

        var books = await unitOfWork
             .RepositoryFactory
             .BookRepository
             .FindAsync(x => x.UserCreatedId == request.Id);

        if (books is null)
            throw new NotFoundException();

        var viewModels = books
            .Select(x => BookViewModel
                .FromEntity(x))
            .ToList();

        return new BaseResultList<BookViewModel>(
            viewModels,
            new PagedResult(1, 1,
            viewModels.Count,
            viewModels.Count));
    }
}
