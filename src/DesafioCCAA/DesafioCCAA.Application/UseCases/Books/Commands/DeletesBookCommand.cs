using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public class DeletesBookCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }
    public Guid UserDeletedId { get; set; }

    public DeletesBookCommand(IEnumerable<Guid> ids, Guid userDeletedId)
    {
        Ids = ids;
        UserDeletedId = userDeletedId;
    }
}