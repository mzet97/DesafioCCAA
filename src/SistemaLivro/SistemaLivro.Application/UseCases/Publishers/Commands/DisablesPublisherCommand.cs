using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands;

public class DisablesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DisablesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}
