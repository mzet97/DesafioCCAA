using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Genders.Commands;

public class AtivesGenderCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public AtivesGenderCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}

