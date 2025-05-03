using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public record DownloadFileQuery(Guid bookId) : IRequest<Stream>;