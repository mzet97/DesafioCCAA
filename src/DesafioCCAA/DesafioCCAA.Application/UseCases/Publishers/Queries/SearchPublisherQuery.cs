using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Shared.Responses;
using DesafioCCAA.Shared.ViewModels;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Queries;

public class SearchPublisherQuery : BaseSearch, IRequest<BaseResultList<PublisherViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
