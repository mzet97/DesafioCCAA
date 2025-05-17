using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands.Handlers;

public class DeletesBookCommandHandler(
    IApplicationUserService applicationUserService,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DeletesBookCommandHandler> logger
    ) :
    IRequestHandler<DeletesBookCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DeletesBookCommand request, CancellationToken cancellationToken)
    {
        var user = await applicationUserService.FindByIdAsync(request.UserDeletedId);

        if(user is null)
        {
            logger.LogError("User not found");
            throw new NotFoundException("Usuário não encontrado");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var id in request.Ids)
            {
                var entity = await unitOfWork.RepositoryFactory
                    .BookRepository
                    .GetByIdAsync(id);

                if (entity is null)
                {
                    logger.LogError("Not found Book");
                    throw new NotFoundException($"Book with id {id} not found");
                }

                entity.Delete(
                    request.UserDeletedId,
                    user
                    );

                unitOfWork.RepositoryFactory
                    .BookRepository
                    .RemoveAsync(entity);

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            await unitOfWork.CommitAsync();

            return new BaseResult(true, "Book(s) deleted successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error deleting Book(s)");
            return new BaseResult(false, "Error deleting Book(s)");
        }
    }
}
