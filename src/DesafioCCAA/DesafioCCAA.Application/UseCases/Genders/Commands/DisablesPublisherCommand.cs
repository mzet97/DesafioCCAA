using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Genders.Commands;

public class DisablesGenderCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DisablesGenderCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}
