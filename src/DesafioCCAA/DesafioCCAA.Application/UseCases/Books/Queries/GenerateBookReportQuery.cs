using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Queries;

public record GenerateBookReportQuery(Guid Id) : IRequest<BaseResult<byte[]>>;
