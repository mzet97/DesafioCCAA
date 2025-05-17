using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Auth.Commands;

public record RequestPasswordResetCommand(
    [property: FromBody] string Email
) : IRequest<BaseResult<bool>>;