using MediatR;

namespace SSO.Business.MailerSettings.Queries
{
    public class GetMailerSettingsQuery : IRequest<object?>
    {
        public Guid RealmId { get; set; }
    }
}
