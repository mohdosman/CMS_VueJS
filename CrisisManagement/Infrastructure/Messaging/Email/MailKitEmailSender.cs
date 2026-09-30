using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CrisisManagement.Infrastructure.Messaging.Email;

// SMTP sender (same as the Blazor CMS). Throws when the relay cannot be reached, so callers can tell
// "sent" from "failed" and report it.
public sealed class MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> log) : IEmailSender
{
    private readonly SmtpOptions _o = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_o.FromName, _o.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtp = new SmtpClient { Timeout = 15000 };
        if (_o.AllowInvalidCertificate)
            smtp.ServerCertificateValidationCallback = static (object _, X509Certificate? _, X509Chain? _, SslPolicyErrors _) => true;

        var security = _o.UseSsl ? SecureSocketOptions.SslOnConnect
            : _o.UseStartTls ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await smtp.ConnectAsync(_o.Host, _o.Port, security, ct);
        if (!string.IsNullOrWhiteSpace(_o.User))
            await smtp.AuthenticateAsync(_o.User, _o.Password, ct);

        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);

        log.LogInformation("Email sent with subject {Subject}", subject);
    }
}
