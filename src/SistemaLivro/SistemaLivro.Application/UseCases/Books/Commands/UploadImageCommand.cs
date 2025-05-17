using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands;

public record UploadImageCommand(
    Guid bookId,
    Guid userUpdatedId,
    Stream FileStream,
    string FileName) : IRequest<BaseResult>;
