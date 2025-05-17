using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands;

public class AtivesPublisherCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesPublisherCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

