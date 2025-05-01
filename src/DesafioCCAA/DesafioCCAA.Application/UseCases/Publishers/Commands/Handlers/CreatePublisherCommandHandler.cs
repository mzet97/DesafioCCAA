using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;

public class CreatePublisherCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<CreatePublisherCommandHandler> logger
    ) : IRequestHandler<CreatePublisherCommand, BaseResult<Guid>>
{

    public async Task<BaseResult<Guid>> Handle(CreatePublisherCommand request, CancellationToken cancellationToken)
    {
        var entity = Publisher.Create(
            request.Name,
            request.Description
        );

        if (!entity.IsValid())
        {
            logger.LogError("Validate Publisher has error");
            throw new ValidationException("Validate Publisher has error", entity.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var publisherRepository = unitOfWork.RepositoryFactory.PublisherRepository;
            await publisherRepository.AddAsync(entity);
            await unitOfWork.CommitAsync();

            foreach (var @event in entity.Events)
            {
                await mediator.Publish(@event);
            }

            return  new BaseResult<Guid>(entity.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to create Publisher");
            await unitOfWork.RollbackAsync();
            throw new Exception("Error to create Publisher", ex);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}
