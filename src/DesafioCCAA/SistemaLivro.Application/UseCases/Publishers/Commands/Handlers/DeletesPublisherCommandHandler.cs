using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands.Handlers;

public class DeletesPublisherCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DeletesPublisherCommandHandler> logger) :
    IRequestHandler<DeletesPublisherCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DeletesPublisherCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach(var id in request.Ids)
            {
                var entity = await unitOfWork.RepositoryFactory
                    .PublisherRepository
                    .GetByIdAsync(id);
                
                if (entity is null)
                {
                    logger.LogError("Not found Publisher");
                    throw new NotFoundException($"Publisher with id {id} not found");
                }

                entity.Delete();
                
                await unitOfWork.RepositoryFactory
                    .PublisherRepository
                     .RemoveByIdAsync(id);

                await unitOfWork.CommitAsync();
                
                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Publisher(s) deleted successfully");
        }
        catch(Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error deleting publisher(s)");
            return new BaseResult(false, "Error deleting publisher(s)");
        }
    }
}
