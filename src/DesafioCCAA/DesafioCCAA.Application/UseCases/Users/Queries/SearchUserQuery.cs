using DesafioCCAA.Application.UseCases.Users.ViewModels;
using DesafioCCAA.Shared.Responses;
using DesafioCCAA.Shared.ViewModels;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Users.Queries;

public class SearchUserQuery : BaseSearch, IRequest<BaseResultList<ApplicationUserViewModel>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
}
