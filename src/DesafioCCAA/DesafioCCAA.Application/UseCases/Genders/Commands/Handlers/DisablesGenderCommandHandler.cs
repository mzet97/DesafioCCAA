using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Genders.Commands.Handlers;

public class DisablesGenderCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DisablesGenderCommandHandler> logger) :
    IRequestHandler<DisablesGenderCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DisablesGenderCommand request, CancellationToken cancellationToken)
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

                entity.Disabled();

                await unitOfWork.RepositoryFactory
                    .GenderRepository
                    .DisableAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Gender(s) disabled successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error disabling Gender(s)");
            return new BaseResult(false, "Error disabling Gender(s)");
        }
    }
}