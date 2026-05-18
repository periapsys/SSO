using MediatR;
using SSO.Infrastructure.Mailer.Enums;
using System.Text.Json.Serialization;

namespace SSO.Business.MailerSettings.Commands
{
    public class ModifyMailerSettingsCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public Guid? RealmId { get; set; }

        public MailerType Type => MailerType.Smtp; // TODO: Smtp for now
        public object Settings { get; set; }
    }
}
