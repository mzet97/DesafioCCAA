using MediatR;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Queries;

public class GetByIdPublisherQuery : IRequest<BaseResult<PublisherViewModel>>
{
    public Guid Id { get; set; }

    public GetByIdPublisherQuery(Guid id)
    {
        Id = id;
    }
}
