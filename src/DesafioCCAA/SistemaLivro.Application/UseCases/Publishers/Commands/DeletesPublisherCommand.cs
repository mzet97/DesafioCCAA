using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands;

public class DeletesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DeletesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}