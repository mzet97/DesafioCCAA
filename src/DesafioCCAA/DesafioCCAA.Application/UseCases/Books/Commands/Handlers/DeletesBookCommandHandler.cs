using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Commands.Handlers;

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

                await unitOfWork.RepositoryFactory
                    .BookRepository
                     .RemoveAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

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
