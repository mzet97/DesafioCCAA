using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;

public class AtivesPublisherCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<AtivesPublisherCommandHandler> logger) :
    IRequestHandler<AtivesPublisherCommand, BaseResult>
{
    public async Task<BaseResult> Handle(AtivesPublisherCommand request, CancellationToken cancellationToken)
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

                entity.Activate();

                await unitOfWork.RepositoryFactory
                    .PublisherRepository
                    .ActiveAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Publisher(s) actived successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error activating publisher(s)");
            return new BaseResult(false, "Error activating publisher(s)");
        }
    }
}
