using MediatR;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;
using SistemaLivro.Shared.ViewModels;

namespace SistemaLivro.Application.UseCases.Publishers.Queries;

public class SearchPublisherQuery : BaseSearch, IRequest<BaseResultList<PublisherViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
