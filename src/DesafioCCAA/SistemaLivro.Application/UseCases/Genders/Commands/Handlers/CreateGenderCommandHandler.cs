using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands.Handlers;

public class CreateGenderCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<CreateGenderCommandHandler> logger
    ) : IRequestHandler<CreateGenderCommand, BaseResult<Guid>>
{

    public async Task<BaseResult<Guid>> Handle(CreateGenderCommand request, CancellationToken cancellationToken)
    {
        var entity = Gender.Create(
            request.Name,
            request.Description
        );

        if (!entity.IsValid())
        {
            logger.LogError("Validate Gender has error");
            throw new ValidationException("Validate Gender has error", entity.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var GenderRepository = unitOfWork.RepositoryFactory.GenderRepository;
            await GenderRepository.AddAsync(entity);
            await unitOfWork.CommitAsync();

            foreach (var @event in entity.Events)
            {
                await mediator.Publish(@event);
            }

            return new BaseResult<Guid>(entity.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to create Gender");
            await unitOfWork.RollbackAsync();
            throw new Exception("Error to create Gender", ex);
        }
    }
}
