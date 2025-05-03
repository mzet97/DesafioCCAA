using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public class AtivesBookCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesBookCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

