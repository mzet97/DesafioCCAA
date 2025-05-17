using MediatR;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Auth.Commands;

public class ConfirmEmailCommand : IRequest<BaseResult<bool>>
{
    public string UserId { get; set; }
    public string Token { get; set; }
}
