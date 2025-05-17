using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Shared.Responses;
using SistemaLivro.Shared.Settings;
using System.Text;

namespace SistemaLivro.Application.UseCases.Auth.Commands.Handlers;

public class RequestPasswordResetCommandHandler(
    IApplicationUserService _userService,
    IEmailSender<ApplicationUser> _emailSender,
    IOptions<AppSettings> appSettings) : IRequestHandler<RequestPasswordResetCommand, BaseResult<bool>>
{

    public async Task<BaseResult<bool>> Handle(RequestPasswordResetCommand req, CancellationToken ct)
    {
        var user = await _userService.FindByEmailAsync(req.Email);
        if (user == null)
            return new BaseResult<bool>(false, false, "Usuário não encontrado.");

        var rawToken = await _userService.GeneratePasswordResetTokenAsync(user);
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var token = WebEncoders.Base64UrlEncode(bytes);

        var link = $"{appSettings.Value.FrontendBaseUrl}/Auth/reset-password" +
                    $"?userId={user.Id}&token={token}";

        await _emailSender.SendPasswordResetLinkAsync(user, req.Email, link);

        return new BaseResult<bool>(true, true, "E-mail enviado com instruções.");
    }
}