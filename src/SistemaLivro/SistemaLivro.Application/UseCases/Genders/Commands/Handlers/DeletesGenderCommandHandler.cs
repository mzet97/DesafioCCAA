using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Genders.Commands;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands.Handlers;

public class DeletesGenderCommandHandler(IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DeletesGenderCommandHandler> logger) :
    IRequestHandler<DeletesGenderCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DeletesGenderCommand request, CancellationToken cancellationToken)
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

                entity.Delete();

                await unitOfWork.RepositoryFactory
                    .GenderRepository
                     .RemoveByIdAsync(id);

                await unitOfWork.CommitAsync();

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new BaseResult(true, "Gender(s) deleted successfully");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error deleting Gender(s)");
            return new BaseResult(false, "Error deleting Gender(s)");
        }
    }
}
