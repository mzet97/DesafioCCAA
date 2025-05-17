using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Commands;

public class DeletesGenderCommand : IRequest<BaseResult>
{
    public IEnumerable<Guid> Ids { get; set; }

    public DeletesGenderCommand(IEnumerable<Guid> ids)
    {
        Ids = ids;
    }
}