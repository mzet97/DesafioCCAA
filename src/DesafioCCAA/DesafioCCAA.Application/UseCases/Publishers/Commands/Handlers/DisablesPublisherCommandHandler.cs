using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;

public class DisablesPublisherCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DisablesPublisherCommandHandler> logger) :
    IRequestHandler<DisablesPublisherCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DisablesPublisherCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var id in request.Ids)
            {
                var entity = await unitOfWork.RepositoryFactory
                    .PublisherRepository
                    .GetByIdAsync(id);

                if (entity is null)
                {
                    logger.LogError("Not found Publisher");
                    throw new NotFoundException($"Publisher with id {id} not found");
                }

                entity.Disabled();

                await unitOfWork.RepositoryFactory
                    .PublisherRepository
                    .DisableAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Publisher(s) disabled successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error disabling publisher(s)");
            return new BaseResult(false, "Error disabling publisher(s)");
        }
    }
}