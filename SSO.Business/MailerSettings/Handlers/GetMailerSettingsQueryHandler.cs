using MediatR;
using Newtonsoft.Json;
using SSO.Business.MailerSettings.Queries;
using SSO.Domain.Management.Interfaces;
using SSO.Infrastructure.Mailer.Dtos;
using SSO.Infrastructure.Mailer.Enums;
using SSO.Infrastructure.Settings.Services;

namespace SSO.Business.MailerSettings.Handlers
{
    public class GetMailerSettingsQueryHandler : IRequestHandler<GetMailerSettingsQuery, object?>
    {
        readonly IRealmMailerSettingsRepository _realmMailerSettingsRepository;
        readonly JwtSecretService _jwtSecretService;
        readonly RsaKeyService _rsaKeyService;

        public GetMailerSettingsQueryHandler(IRealmMailerSettingsRepository realmMailerSettingsRepository,
            JwtSecretService jwtSecretService,
            RsaKeyService rsaKeyService)
        {
            _realmMailerSettingsRepository = realmMailerSettingsRepository;
            _jwtSecretService = jwtSecretService;
            _rsaKeyService = rsaKeyService;
        }

        public async Task<object?> Handle(GetMailerSettingsQuery request, CancellationToken cancellationToken)
        {
            var rec = await _realmMailerSettingsRepository.FindOne(x => x.RealmId == request.RealmId);

            if (rec is not null)
            {
                var decStr = _rsaKeyService.DecryptString(rec.Value, _jwtSecretService.PrivateKey);

                object value = rec.MailerType switch
                {
                    MailerType.Smtp => JsonConvert.DeserializeObject<SmtpDto>(decStr)!,
                    _ => throw new NotImplementedException()
                };

                return value;
            }

            return null;
        }
    }
}
