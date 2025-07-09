using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands;

public class DeleteMultiplePublishersCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DeleteMultiplePublishersCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }

    public DeleteMultiplePublishersCommand()
    {
        Ids = new List<Guid>();
    }
}
