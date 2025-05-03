using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Commands.Handlers;

public class UpdateBookCommandHandler(
    IApplicationUserService applicationUserService,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<UpdateBookCommandHandler> logger) :
    IRequestHandler<UpdateBookCommand, BaseResult<Guid>>
{
    public async Task<BaseResult<Guid>> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var user = await applicationUserService.FindByIdAsync(request.UserUpdatedId);

        if (user is null)
        {
            logger.LogError("User not found");
            throw new NotFoundException("Usuário não encontrado");
        }

        var entityDb = await unitOfWork.RepositoryFactory.BookRepository.GetByIdAsync(request.Id);

        if (entityDb is null)
        {
            logger.LogError("Not found Book");
            throw new NotFoundException();
        }

        var gender = await unitOfWork
            .RepositoryFactory
            .GenderRepository
            .GetByIdAsync(request.GenderId);

        if (gender is null)
        {
            logger.LogError("Gender not found");
            throw new NotFoundException("Gênero não encontrado");
        }

        var publisher = await unitOfWork
            .RepositoryFactory
            .PublisherRepository
            .GetByIdAsync(request.PublisherId);

        if (publisher is null)
        {
            logger.LogError("Publisher not found");
            throw new NotFoundException("Editora não encontrado");
        }

        entityDb.Update(
            request.Title,
            request.Author,
            request.Synopsis,
            request.ISBN,
            entityDb.CoverImage,
            request.GenderId,
            gender,
            request.PublisherId,
            publisher,
            request.UserUpdatedId,
            user
            );

        if (!entityDb.IsValid())
        {
            logger.LogError("Validate Book has error");
            throw new ValidationException("Validate Book has error", entityDb.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var BookRepository = unitOfWork.RepositoryFactory.BookRepository;

            await BookRepository.UpdateAsync(entityDb);

            await unitOfWork.CommitAsync();

            foreach (var @event in entityDb.Events)
            {
                await mediator.Publish(@event);
            }

            return new BaseResult<Guid>(entityDb.Id, true, "Livro atualizado com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to update Book");
            await unitOfWork.RollbackAsync();
            return new BaseResult<Guid>(entityDb.Id, false, "Erro ao atualizar o livro");
        }
    }
}
