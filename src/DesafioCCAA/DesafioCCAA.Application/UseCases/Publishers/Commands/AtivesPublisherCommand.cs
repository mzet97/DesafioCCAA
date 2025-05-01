using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands;

public class AtivesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

