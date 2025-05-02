using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Shared.Responses;
using DesafioCCAA.Shared.ViewModels;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Genders.Queries;

public class SearchGenderQuery : BaseSearch, IRequest<BaseResultList<GenderViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
