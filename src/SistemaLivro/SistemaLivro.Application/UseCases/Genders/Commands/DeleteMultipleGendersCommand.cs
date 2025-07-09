using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands;

public class DeleteMultipleGendersCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }
    public Guid UserDeletedId { get; set; }

    public DeleteMultipleGendersCommand(IEnumerable<Guid> ids, Guid userDeletedId)
    {
        Ids = ids;
        UserDeletedId = userDeletedId;
    }
}
