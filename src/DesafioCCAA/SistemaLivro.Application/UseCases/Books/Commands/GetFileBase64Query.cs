using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands;

public record GetFileBase64Query(Guid bookId) : IRequest<BaseResult<string>>;

