using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SSO.Business.MailerSettings.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Infrastructure.Mailer.Dtos;
using SSO.Infrastructure.Mailer.Enums;
using SSO.Infrastructure.Settings.Services;
using System.Security.Cryptography;
using System.Text.Json;

namespace SSO.Business.MailerSettings.Handlers
{
    public class ModifyMailerSettingsCommandHandler : IRequestHandler<ModifyMailerSettingsCommand, Unit>
    {
        readonly IRealmMailerSettingsRepository _realmMailerSettingsRepository;
        readonly JwtSecretService _jwtSecretService;
        readonly RsaKeyService _rsaKeyService;

        public ModifyMailerSettingsCommandHandler(IRealmMailerSettingsRepository realmMailerSettingsRepository,
            JwtSecretService jwtSecretService,
            RsaKeyService rsaKeyService)
        {
            _realmMailerSettingsRepository = realmMailerSettingsRepository;
            _jwtSecretService = jwtSecretService;
            _rsaKeyService = rsaKeyService;
        }

        public async Task<Unit> Handle(ModifyMailerSettingsCommand request, CancellationToken cancellationToken)
        {
            var jsonElement = (JsonElement)request.Settings;

            object? data = request.Type switch
            {
                MailerType.Smtp => jsonElement.Deserialize<SmtpDto>(),
                _ => throw new NotImplementedException()
            };

            if (data is null)
                throw new ArgumentException("Invalid mailer settings");

            var rec = await _realmMailerSettingsRepository.FindOne(x => x.RealmId == request.RealmId);

            if (rec is not null)
                await _realmMailerSettingsRepository.Delete(rec, false);

            var publicKey = RSA.Create(); publicKey.ImportParameters(_jwtSecretService.PrivateKey.ExportParameters(false));
            
            // Replace the original serialization line with the following to produce camelCase JSON:
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var secret = _rsaKeyService.EncryptString(JsonConvert.SerializeObject(data, serializerSettings), publicKey);

            var entry = new Domain.Models.RealmMailerSettings
            {
                RealmId = request.RealmId!.Value,
                MailerType = request.Type,
                Value = secret
            };

            await _realmMailerSettingsRepository.Add(entry);

            return new();
        }
    }
}
