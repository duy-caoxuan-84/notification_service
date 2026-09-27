using MailKit.Net.Smtp;
using MimeKit;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Providers;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly string _host;
    private readonly int _port;

    public SmtpEmailProvider(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Notification Service", "noreply@internal.system"));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
