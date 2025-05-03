using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public record UploadImageCommand(
    Guid bookId,
    Guid userUpdatedId,
    Stream FileStream,
    string FileName) : IRequest<BaseResult>;
