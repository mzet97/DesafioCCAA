using DesafioCCAA.Application.UseCases.Auth.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Auth.Commands;

public class LoginUserCommand : IRequest<BaseResult<LoginResponseViewModel>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
