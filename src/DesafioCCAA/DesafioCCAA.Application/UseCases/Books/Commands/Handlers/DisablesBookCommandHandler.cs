using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Commands.Handlers;

public class DisablesBookCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DisablesBookCommandHandler> logger) :
    IRequestHandler<DisablesBookCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DisablesBookCommand request, CancellationToken cancellationToken)
    {
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

                entity.Disabled();

                await unitOfWork.RepositoryFactory
                    .BookRepository
                    .DisableAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Book(s) disabled successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error disabling Book(s)");
            return new BaseResult(false, "Error disabling Book(s)");
        }
    }
}