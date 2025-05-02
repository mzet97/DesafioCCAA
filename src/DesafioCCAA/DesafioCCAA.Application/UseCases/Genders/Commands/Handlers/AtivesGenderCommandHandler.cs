using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Genders.Commands.Handlers;

public class AtivesGenderCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<AtivesGenderCommandHandler> logger) :
    IRequestHandler<AtivesGenderCommand, BaseResult>
{
    public async Task<BaseResult> Handle(AtivesGenderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var id in request.Ids)
            {
                var entity = await unitOfWork.RepositoryFactory
                    .GenderRepository
                    .GetByIdAsync(id);

                if (entity is null)
                {
                    logger.LogError("Not found Gender");
                    throw new NotFoundException($"Gender with id {id} not found");
                }

                entity.Activate();

                await unitOfWork.RepositoryFactory
                    .GenderRepository
                    .ActiveAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Gender(s) actived successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error activating Gender(s)");
            return new BaseResult(false, "Error activating Gender(s)");
        }
    }
}
