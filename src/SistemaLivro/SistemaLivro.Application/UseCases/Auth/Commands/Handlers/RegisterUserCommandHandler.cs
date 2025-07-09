using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Application.UseCases.Auth.ViewModels;
using SistemaLivro.Shared.Responses;
using System.Text;

namespace SistemaLivro.Application.UseCases.Auth.Commands.Handlers;

public class RegisterUserCommandHandler(
    IApplicationUserService applicationUserService,
    ILogger<RegisterUserCommandHandler> logger,
    IEmailSender<ApplicationUser> emailSender,
    IMediator mediator
    ) : IRequestHandler<RegisterUserCommand, BaseResult<LoginResponseViewModel>>
{
    public async Task<BaseResult<LoginResponseViewModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = request.ToDomain();

        var resultCreateUser = await applicationUserService.CreateAsync(user, request.Password);

        if (resultCreateUser.Succeeded)
        {
            var rawToken = await applicationUserService.GenerateEmailConfirmationTokenAsync(user);
            var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
            var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

            var confirmationLink = $"http://localhost:5120/Auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await emailSender.SendConfirmationLinkAsync(user, request.Email, confirmationLink);

            // For development/testing purposes, automatically confirm email
            var confirmResult = await applicationUserService.ConfirmEmailAsync(user, rawToken);
            if (!confirmResult.Succeeded)
            {
                logger.LogWarning("Failed to auto-confirm email for user {Email}", request.Email);
            }

            await applicationUserService.TrySignInAsync(user);
            return await mediator.Send(new GetTokenCommand { Email = request.Email });
        }

        var sb = new StringBuilder();
        foreach (var error in resultCreateUser.Errors)
        {
            sb.Append(error.Description);
        }

        logger.LogInformation($"Falha: {sb.ToString()}");

        return new BaseResult<LoginResponseViewModel>(null, false, sb.ToString());
    }
}
