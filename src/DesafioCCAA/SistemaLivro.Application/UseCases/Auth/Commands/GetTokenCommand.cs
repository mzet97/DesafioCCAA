using MediatR;
using SistemaLivro.Application.UseCases.Auth.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Auth.Commands;

public class GetTokenCommand :
    IRequest<BaseResult<LoginResponseViewModel>>
{
    public string Email { get; set; } = string.Empty;
}
