using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Queries;

public class GetByIdBookQuery : IRequest<BaseResult<BookViewModel>>
{
    public Guid Id { get; set; }

    public GetByIdBookQuery(Guid id)
    {
        Id = id;
    }
}
