using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands.Handlers;

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
    }
}
