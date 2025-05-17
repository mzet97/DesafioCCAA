using MediatR;
using SistemaLivro.Application.UseCases.Genders.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Genders.Queries;

public class GetByIdGenderQuery : IRequest<BaseResult<GenderViewModel>>
{
    public Guid Id { get; set; }

    public GetByIdGenderQuery(Guid id)
    {
        Id = id;
    }
}
