using DesafioCCAA.Shared.Settings;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DesafioCCAA.Infrastructure.Email;

public class SendEmail : ISendEmail
{
    private readonly SmtpSettings _smtp;
    private readonly IWebHostEnvironment _env;

    public SendEmail(IOptions<SmtpSettings> opts, IWebHostEnvironment env)
    {
        _smtp = opts.Value;
        _env = env;
    }

    public async Task SendEmailAsync(string email, string subject, string body)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
        msg.To.Add(new MailboxAddress("", email));
        msg.Subject = subject;
        msg.Body = new TextPart("html") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();

        client.ServerCertificateValidationCallback = (_, _, _, _) => true;

        var socketOpts = _env.IsDevelopment()
            ? MailKit.Security.SecureSocketOptions.None
            : MailKit.Security.SecureSocketOptions.StartTls;

        await client.ConnectAsync(_smtp.Server, _smtp.Port, socketOpts);

        if (!string.IsNullOrWhiteSpace(_smtp.Username))
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}
