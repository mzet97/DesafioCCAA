using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Application.UseCases.Auth.Commands;

public record RequestPasswordResetCommand(
    [property: FromBody] string Email
) : IRequest<BaseResult<bool>>;