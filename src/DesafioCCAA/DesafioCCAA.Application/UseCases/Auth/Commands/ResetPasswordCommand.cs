using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Application.UseCases.Auth.Commands;
public record ResetPasswordCommand(
    [property: FromBody] string UserId,
    [property: FromBody] string Token,
    [property: FromBody] string NewPassword
) : IRequest<BaseResult<bool>>;