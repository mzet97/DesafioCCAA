using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Genders.Queries.Handlers;

public class GetByIdGenderQueryHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<GetByIdGenderQuery, BaseResult<GenderViewModel>>
{
    public async Task<BaseResult<GenderViewModel>> Handle(GetByIdGenderQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var Gender = await unitOfWork
            .RepositoryFactory
            .GenderRepository
            .GetByIdAsync(request.Id);

        if (Gender is null)
            throw new NotFoundException("Not found");

        return new BaseResult<GenderViewModel>(
            GenderViewModel
            .FromEntity(Gender));
    }
}
