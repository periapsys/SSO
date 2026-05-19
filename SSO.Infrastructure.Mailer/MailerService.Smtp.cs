using MailKit.Net.Smtp;
using MimeKit;
using SSO.Infrastructure.Mailer.Dtos;

namespace SSO.Infrastructure.Mailer
{
    public partial class MailerService
    {
        async Task SendViaSmtp(object settings, string fromEmail, string fromName, string toEmail, string subject, string body)
        {
            var smtpSettings = settings as SmtpDto;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpSettings!.SmtpServer, smtpSettings!.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpSettings!.Username, smtpSettings!.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
