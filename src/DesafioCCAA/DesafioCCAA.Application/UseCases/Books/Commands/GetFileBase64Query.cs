using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public record GetFileBase64Query(Guid bookId) : IRequest<BaseResult<string>>;

