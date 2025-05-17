using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Shared.Responses;
using System.Text;

namespace SistemaLivro.Application.UseCases.Auth.Commands.Handlers;

public class ConfirmEmailCommandHandler(
    IApplicationUserService applicationUserService,
    ILogger<ConfirmEmailCommandHandler> logger,
    IEmailSender<ApplicationUser> emailSender
    ) : IRequestHandler<ConfirmEmailCommand, BaseResult<bool>>
{
    public async Task<BaseResult<bool>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.Parse(request.UserId);
        var user = await applicationUserService.FindByIdAsync(id);

        if (user is null)
        {
            logger.LogInformation($"Usuário não encontrado: {request.UserId}");
            return new BaseResult<bool>(false, false, "Usuário não encontrado.");
        }

        var decodedBytes = WebEncoders.Base64UrlDecode(request.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedBytes);

        var result = await applicationUserService.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            logger.LogInformation($"Email confirmado com sucesso: {request.UserId}");
            return new BaseResult<bool>(true, true, "Email confirmado com sucesso.");
        }

        return new BaseResult<bool>(false, false, "Token inválido.");
    }
}
