using DesafioCCAA.Application.UseCases.Auth.ViewModels;
using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DesafioCCAA.Application.UseCases.Auth.Commands.Handlers;

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
