using MediatR;
using SistemaLivro.Application.UseCases.Users.ViewModels;
using SistemaLivro.Shared.Responses;
using SistemaLivro.Shared.ViewModels;

namespace SistemaLivro.Application.UseCases.Users.Queries;

public class SearchUserQuery : BaseSearch, IRequest<BaseResultList<ApplicationUserViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
}
