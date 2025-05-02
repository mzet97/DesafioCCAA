using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Genders.Queries;

public class GetByIdGenderQuery : IRequest<BaseResult<GenderViewModel>>
{
    public Guid Id { get; set; }

    public GetByIdGenderQuery(Guid id)
    {
        Id = id;
    }
}
