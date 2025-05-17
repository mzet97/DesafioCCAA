using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands;

public class AtivesBookCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesBookCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

