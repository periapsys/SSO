using MediatR;
using SSO.Business.MailerSettings.Commands;
using SSO.Domain.Management.Interfaces;

namespace SSO.Business.MailerSettings.Handlers
{
    public class RemoveMailerSettingsCommandHandler : IRequestHandler<RemoveMailerSettingsCommand, Unit>
    {
        readonly IRealmMailerSettingsRepository _repository;

        public RemoveMailerSettingsCommandHandler(IRealmMailerSettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(RemoveMailerSettingsCommand request, CancellationToken cancellationToken)
        {
            var rec = await _repository.FindOne(x => x.RealmId == request.RealmId);

            if (rec is null)
                throw new ArgumentNullException();

            await _repository.Delete(rec);

            return new();
        }
    }
}
