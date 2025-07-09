using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Genders.Commands;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Services.Interfaces;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands.Handlers;

public class DeleteMultipleGendersCommandHandler(
    IApplicationUserService applicationUserService,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<DeleteMultipleGendersCommandHandler> logger
    ) :
    IRequestHandler<DeleteMultipleGendersCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DeleteMultipleGendersCommand request, CancellationToken cancellationToken)
    {
        var user = await applicationUserService.FindByIdAsync(request.UserDeletedId);

        if (user is null)
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
                    .GenderRepository
                    .GetByIdAsync(id);

                if (entity is null)
                {
                    logger.LogError("Not found Gender");
                    throw new NotFoundException($"Gender with id {id} not found");
                }

                entity.Delete();

                unitOfWork.RepositoryFactory
                    .GenderRepository
                    .RemoveAsync(entity);

                foreach (var domainEvent in entity.Events)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }

            await unitOfWork.CommitAsync();

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
