using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Auth.Commands;
public record ResetPasswordCommand(
    [property: FromBody] string UserId,
    [property: FromBody] string Token,
    [property: FromBody] string NewPassword
) : IRequest<BaseResult<bool>>;