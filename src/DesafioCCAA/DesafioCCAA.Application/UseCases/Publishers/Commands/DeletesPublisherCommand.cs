using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Commands;

public class DeletesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DeletesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}