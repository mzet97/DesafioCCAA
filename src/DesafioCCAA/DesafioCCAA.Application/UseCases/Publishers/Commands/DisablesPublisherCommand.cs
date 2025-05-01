using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands;

public class DisablesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DisablesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}
