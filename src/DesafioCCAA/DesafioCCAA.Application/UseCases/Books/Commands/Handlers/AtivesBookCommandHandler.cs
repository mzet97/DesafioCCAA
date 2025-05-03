using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Commands.Handlers;

public class AtivesBookCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<AtivesBookCommandHandler> logger) :
    IRequestHandler<AtivesBookCommand, BaseResult>
{
    public async Task<BaseResult> Handle(AtivesBookCommand request, CancellationToken cancellationToken)
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

                entity.Activate();

                await unitOfWork.RepositoryFactory
                    .BookRepository
                    .ActiveAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Book(s) actived successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error activating Book(s)");
            return new BaseResult(false, "Error activating Book(s)");
        }
    }
}
