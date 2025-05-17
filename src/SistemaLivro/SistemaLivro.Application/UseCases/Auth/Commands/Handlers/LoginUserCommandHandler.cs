using SistemaLivro.Domain.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Auth.Commands;
using SistemaLivro.Application.UseCases.Auth.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Auth.Commands.Handlers;

public class LoginUserCommandHandler(
    IApplicationUserService applicationUserService,
    ILogger<LoginUserCommandHandler> logger,
    IMediator mediator)
    : IRequestHandler<LoginUserCommand, BaseResult<LoginResponseViewModel>>
{
    public async Task<BaseResult<LoginResponseViewModel>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var result = await applicationUserService.PasswordSignInByEmailAsync(request.Email, request.Password, false, false);

        if (result.Succeeded)
        {
            return await mediator.Send(new GetTokenCommand { Email = request.Email });
        }
        else if (result.IsLockedOut)
        {
            logger.LogInformation("Falha: Login bloqueado");
            return new BaseResult<LoginResponseViewModel>(null, false, "Falha: Login bloqueado");
        }


        logger.LogInformation("Falha: Erro ao fazer login");

        return new BaseResult<LoginResponseViewModel>(null, false, "Falha: Erro ao fazer login");
    }
}
