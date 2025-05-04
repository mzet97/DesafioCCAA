using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Books.Commands.Handlers;

public class GetFileBase64QueryHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    ILogger<GetFileBase64QueryHandler> logger
    ) : IRequestHandler<GetFileBase64Query, BaseResult<string>>
{
    public async Task<BaseResult<string>> Handle(GetFileBase64Query request, CancellationToken cancellationToken)
    {
        var book = await unitOfWork
            .RepositoryFactory
            .BookRepository
            .GetByIdNoTrackingAsync(request.bookId);

        if (book is null)
        {
            logger.LogError("Book not found");
            throw new NotFoundException("Livro não encontrado");
        }

        var base64 = await fileService
            .GetFileBase64Async(book.CoverImage.FileName);

        if (base64 is null)
        {
            logger.LogError("File not found");
            throw new NotFoundException("Arquivo não encontrado");
        }

        return new BaseResult<string>(base64);
    }
}