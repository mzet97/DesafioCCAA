using MediatR;
using SistemaLivro.Application.UseCases.Genders.ViewModels;
using SistemaLivro.Shared.Responses;
using SistemaLivro.Shared.ViewModels;

namespace SistemaLivro.Application.UseCases.Genders.Queries;

public class SearchGenderQuery : BaseSearch, IRequest<BaseResultList<GenderViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
