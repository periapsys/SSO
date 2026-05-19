using MediatR;
using SSO.Infrastructure.Mailer.Enums;

namespace SSO.Business.MailerSettings.Queries
{
    public class TestMailerSettingsQuery : IRequest<Unit>
    {
        public MailerType Type => MailerType.Smtp; // TODO: Smtp for now
        public object Settings { get; set; }
        public string ToEmail { get; set; }
    }
}
