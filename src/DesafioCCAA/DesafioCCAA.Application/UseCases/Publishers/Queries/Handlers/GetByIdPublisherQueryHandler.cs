using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Publishers.Queries.Handlers;

public class GetByIdPublisherQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetByIdPublisherQuery, BaseResult<PublisherViewModel>>
{
    public async Task<BaseResult<PublisherViewModel>> Handle(GetByIdPublisherQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var publisher = await unitOfWork
            .RepositoryFactory
            .PublisherRepository
            .GetByIdAsync(request.Id);

        if (publisher is null)
            throw new NotFoundException("Not found");

        return new BaseResult<PublisherViewModel>(
            PublisherViewModel
            .FromEntity(publisher));
    }
}
