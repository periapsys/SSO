using MediatR;

namespace SSO.Business.MailerSettings.Commands
{
    public class RemoveMailerSettingsCommand : IRequest<Unit>
    {
        public Guid RealmId { get; set; }
    }
}
