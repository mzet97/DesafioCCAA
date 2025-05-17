using SistemaLivro.Domain.Domains.Identities;
using Microsoft.AspNetCore.Identity;

namespace SistemaLivro.Infrastructure.Email;


public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly ISendEmail _sendEmail;

    public EmailSender(ISendEmail sendEmail)
    {
        _sendEmail = sendEmail;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirmação de e-mail";
        var htmlBody = $@"
            <p>Olá {user.UserName},</p>
            <p>Para ativar sua conta, clique no link abaixo:</p>
            <p><a href=""{confirmationLink}"">Confirmar meu e-mail</a></p>
            <p>Se você não solicitou este e-mail, simplesmente ignore.</p>
        ";

        await _sendEmail.SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _sendEmail.SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Código para redefinição de senha";
        var htmlBody = $@"
            <p>Olá {user.UserName},</p>
            <p>Use o código abaixo para redefinir sua senha:</p>
            <h2>{resetCode}</h2>
            <p>Este código expira em XX minutos.</p>
            <p>Se você não solicitou essa ação, ignore este e-mail.</p>
        ";

        await _sendEmail.SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Link para redefinição de senha";
        var htmlBody = $@"
            <p>Olá {user.UserName},</p>
            <p>Para redefinir sua senha, clique no link abaixo:</p>
            <p><a href=""{resetLink}"">Redefinir minha senha</a></p>
            <p>Este link expira em XX horas.</p>
            <p>Se você não solicitou essa ação, ignore este e-mail.</p>
        ";

        await _sendEmail.SendEmailAsync(email, subject, htmlBody);
    }
}
