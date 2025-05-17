using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using SistemaLivro.Shared.Responses;
using System.Text;

namespace SistemaLivro.Application.UseCases.Auth.Commands.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, BaseResult<bool>>
{
    private readonly IApplicationUserService _userService;
    public ResetPasswordCommandHandler(IApplicationUserService userService)
        => _userService = userService;

    public async Task<BaseResult<bool>> Handle(ResetPasswordCommand req, CancellationToken ct)
    {
        if (!Guid.TryParse(req.UserId, out var id))
            return new BaseResult<bool>(false, false, "UserId inválido.");

        var user = await _userService.FindByIdAsync(id);
        if (user is null)
            return new BaseResult<bool>(false, false, "Usuário não encontrado.");

        var decoded = WebEncoders.Base64UrlDecode(req.Token);
        var rawToken = Encoding.UTF8.GetString(decoded);

        var identityResult =
            await _userService.ResetPasswordAsync(user, rawToken, req.NewPassword);

        if (identityResult.Succeeded)
            return new BaseResult<bool>(true, true, "Senha redefinida com sucesso.");

        var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
        return new BaseResult<bool>(false, false, errors);
    }
}