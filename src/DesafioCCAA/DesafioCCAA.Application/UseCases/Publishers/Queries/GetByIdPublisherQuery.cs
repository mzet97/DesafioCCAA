using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Queries;

public class GetByIdPublisherQuery : IRequest<BaseResult<PublisherViewModel>>
{
    public Guid Id { get; set; }

    public GetByIdPublisherQuery(Guid id)
    {
        Id = id;
    }
}
