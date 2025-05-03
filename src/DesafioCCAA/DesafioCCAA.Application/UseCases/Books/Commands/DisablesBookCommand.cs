using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public class DisablesBookCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DisablesBookCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}
