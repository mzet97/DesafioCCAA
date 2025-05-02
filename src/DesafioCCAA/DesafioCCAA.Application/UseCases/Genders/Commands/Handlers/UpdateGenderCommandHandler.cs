using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Genders.Commands.Handlers;

public class UpdateGenderCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<UpdateGenderCommandHandler> logger) :
    IRequestHandler<UpdateGenderCommand, BaseResult<Guid>>
{
    public async Task<BaseResult<Guid>> Handle(UpdateGenderCommand request, CancellationToken cancellationToken)
    {
        var entityDb = await unitOfWork.RepositoryFactory.GenderRepository.GetByIdAsync(request.Id);

        if (entityDb is null)
        {
            logger.LogError("Not found Gender");
            throw new NotFoundException();
        }

        entityDb.Update(request.Name, request.Description);

        if (!entityDb.IsValid())
        {
            logger.LogError("Validate Gender has error");
            throw new ValidationException("Validate Gender has error", entityDb.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var GenderRepository = unitOfWork.RepositoryFactory.GenderRepository;

            await GenderRepository.UpdateAsync(entityDb);

            unitOfWork.CommitAsync().Wait();

            foreach (var @event in entityDb.Events)
            {
                mediator.Publish(@event).Wait();
            }

            return new BaseResult<Guid>(entityDb.Id, true, "Gender updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to update Gender");
            await unitOfWork.RollbackAsync();
            return new BaseResult<Guid>(entityDb.Id, false, "Error to update Gender");
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}
