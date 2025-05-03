using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;

public class UpdatePublisherCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<UpdatePublisherCommandHandler> logger) :
    IRequestHandler<UpdatePublisherCommand, BaseResult<Guid>>
{
    public async Task<BaseResult<Guid>> Handle(UpdatePublisherCommand request, CancellationToken cancellationToken)
    {
       var entityDb = await unitOfWork.RepositoryFactory.PublisherRepository.GetByIdAsync(request.Id);
        
        if (entityDb is null)
        {
            logger.LogError("Not found Publisher");
            throw new NotFoundException();
        }
        
        entityDb.Update(request.Name, request.Description);
        
        if (!entityDb.IsValid())
        {
            logger.LogError("Validate Publisher has error");
            throw new ValidationException("Validate Publisher has error", entityDb.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var publisherRepository = unitOfWork.RepositoryFactory.PublisherRepository;
            
            await publisherRepository.UpdateAsync(entityDb);
            
            await unitOfWork.CommitAsync();
            
            foreach (var @event in entityDb.Events)
            {
                await mediator.Publish(@event);
            }
            
            return new BaseResult<Guid>(entityDb.Id, true, "Publisher updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to update Publisher");
            await unitOfWork.RollbackAsync();
            return new BaseResult<Guid>(entityDb.Id, false, "Error to update Publisher");
        }
    }
}
