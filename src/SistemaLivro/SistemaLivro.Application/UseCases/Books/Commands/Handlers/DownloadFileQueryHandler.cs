using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Books.Commands;

namespace SistemaLivro.Application.UseCases.Books.Commands.Handlers;

public class DownloadFileQueryHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    ILogger<DownloadFileQueryHandler> logger
    ) : IRequestHandler<DownloadFileQuery, Stream>
{
    public async Task<Stream> Handle(
        DownloadFileQuery request,
        CancellationToken cancellationToken)
    {
        var book = await unitOfWork
            .RepositoryFactory
            .BookRepository
            .GetByIdAsync(request.bookId);

        if (book is null)
        {
            logger.LogError("Book not found");
            throw new NotFoundException("Livro não encontrado");
        }

        var file = await fileService
            .GetFileStreamAsync(book.CoverImage.FileName);

        if (file is null)
        {
            logger.LogError("File not found");
            throw new NotFoundException("Arquivo não encontrado");
        }

        return file;
    }
}