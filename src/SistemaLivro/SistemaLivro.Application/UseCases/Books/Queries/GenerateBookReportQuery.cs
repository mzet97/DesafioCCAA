using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Queries;

public record GenerateBookReportQuery(Guid Id) : IRequest<BaseResult<byte[]>>;
