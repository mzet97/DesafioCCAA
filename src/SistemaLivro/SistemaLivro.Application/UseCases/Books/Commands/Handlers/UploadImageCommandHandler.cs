using SistemaLivro.Domain.Domains.Books.ValueObjects;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands.Handlers;

public class UploadImageCommandHandler(
    IApplicationUserService applicationUserService,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IFileService fileService,
    ILogger<UploadImageCommandHandler> logger

    ) : IRequestHandler<UploadImageCommand, BaseResult>
{

    public async Task<BaseResult> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var user = await applicationUserService
            .FindByIdAsync(request.userUpdatedId);

        if(user is null)
        {
            logger.LogError("User not found");
            throw new NotFoundException("Usuário não encontrado");
        }

        var book = await unitOfWork
            .RepositoryFactory
            .BookRepository
            .GetByIdAsync(request.bookId);

        if(book is null)
        {
            logger.LogError("Book not found");
            throw new NotFoundException("Livro não encontrado");
        }

        var (fullPath, uniqueName) = await fileService
            .SaveImageAsync(
            request.FileStream,
            request.FileName);

        var coverImage = new CoverImage(uniqueName, fullPath);

        book.UpdateImage(
            coverImage,
            request.userUpdatedId,
            user);

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var BookRepository = unitOfWork.RepositoryFactory.BookRepository;

            await BookRepository.UpdateAsync(book);

            await unitOfWork.CommitAsync();

            foreach (var @event in book.Events)
            {
                await mediator.Publish(@event);
            }

            return new BaseResult<Guid>(book.Id, true, "Imagem do livro atualizado com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to update Book");
            await unitOfWork.RollbackAsync();
            return new BaseResult<Guid>(book.Id, false, "Erro ao atualizar a imagem do livro");
        }
    }
}
