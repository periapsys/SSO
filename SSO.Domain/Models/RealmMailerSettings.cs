using SSO.Infrastructure.Mailer.Enums;

namespace SSO.Domain.Models
{
    public class RealmMailerSettings
    {
        public Guid RealmId { get; set; }
        public MailerType MailerType { get; set; }
        public string Value { get; set; }

        public virtual Realm Realm { get; set; }
    }
}
