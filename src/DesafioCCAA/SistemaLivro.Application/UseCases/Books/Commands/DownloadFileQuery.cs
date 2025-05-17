using MediatR;

namespace SistemaLivro.Application.UseCases.Books.Commands;

public record DownloadFileQuery(Guid bookId) : IRequest<Stream>;