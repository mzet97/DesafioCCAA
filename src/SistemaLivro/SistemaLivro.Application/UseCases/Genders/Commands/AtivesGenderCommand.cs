using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands;

public class AtivesGenderCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesGenderCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

