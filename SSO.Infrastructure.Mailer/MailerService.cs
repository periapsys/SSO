using SSO.Infrastructure.Mailer.Enums;

namespace SSO.Infrastructure.Mailer
{
    public partial class MailerService
    {
        readonly IReadOnlyDictionary<MailerType, Func<object, string, string, string, string, string, Task>> _senders;

        public MailerService()
        {
            _senders = new Dictionary<MailerType, Func<object, string, string, string, string, string, Task>>
            {
                { MailerType.Smtp, (settings, fromEmail, fromName, toEmail, subject, body) => SendViaSmtp(settings, fromEmail, fromName, toEmail, subject, body) }
            };

        }

        public async Task SendEmailAsync(MailerType mailerType, object settings, string fromEmail, string fromName, string toEmail, string subject, string body)
        {
            await _senders[mailerType](settings, fromEmail, fromName, toEmail, subject, body);
        }
    }
}
