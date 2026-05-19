using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Authentication.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;
using SSO.Infrastructure.Mailer;
using SSO.Infrastructure.Mailer.Dtos;
using SSO.Infrastructure.Mailer.Enums;
using SSO.Infrastructure.Settings.Services;
using System.Text.Json;

namespace SSO.Business.Authentication.Handlers
{
    public class PasswordRecoveryCommandHandler : IRequestHandler<PasswordRecoveryCommand, Unit>
    {
        readonly IMemoryCache _cache;
        readonly IRealmRepository _realmRepository;
        readonly UserManager<ApplicationUser> _userManager;
        readonly MailerService _mailerService;
        readonly JwtSecretService _jwtSecretService;
        readonly RsaKeyService _rsaKeyService;

        public PasswordRecoveryCommandHandler(IMemoryCache cache, 
            IRealmRepository realmRepository, 
            UserManager<ApplicationUser> userManager,
            MailerService mailerService,
            JwtSecretService jwtSecretService,
            RsaKeyService rsaKeyService)
        {
            _cache = cache;
            _realmRepository = realmRepository;
            _userManager = userManager;
            _mailerService = mailerService;
            _jwtSecretService = jwtSecretService;
            _rsaKeyService = rsaKeyService;
        }

        public async Task<Unit> Handle(PasswordRecoveryCommand request, CancellationToken cancellationToken)
        {
            var realm = await _realmRepository.FindOne(x => x.RealmId == request.RealmId);
            realm ??= await _realmRepository.FindOne(x => x.Name == "Default");

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                if (realm.RealmMailerSettings != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var id = Guid.NewGuid().ToString();

                    _cache.Set($"recovery:{id}", token, TimeSpan.FromMinutes(30));

                    var resetLink = $"{request.BaseUrl}/resetpassword?userId={user.Id}&token={id}";

                    var decStr = _rsaKeyService.DecryptString(realm.RealmMailerSettings.Value, _jwtSecretService.PrivateKey);

                    (object settings, string username) parameters = realm.RealmMailerSettings.MailerType switch
                    {
                        MailerType.Smtp => GetParameters<SmtpDto>(decStr),
                        _ => throw new NotSupportedException($"MailerType '{realm.RealmMailerSettings.MailerType}' is not supported.")
                    };

                    await _mailerService.SendEmailAsync(
                        mailerType: MailerType.Smtp,
                        settings: parameters.settings,
                        fromEmail: parameters.username,
                        fromName: "SSO",
                        toEmail: request.Email,
                        subject: "Password Recovery",
                        body: $"<html><body><p>Click the link below to reset your password:</p><p><a href=\"{resetLink}\">Reset your password</a></p></body></html>");
                }
            }

            return new();
        }

        private (T settings, string username) GetParameters<T>(string settingsStr) where T : class
        {
            var obj = JsonSerializer.Deserialize<T>(settingsStr) ?? throw new ArgumentException("Invalid mailer settings.");

            return obj switch
            {
                SmtpDto s => (settings: obj, username: s.Username),
                // TODO: Add other DTO cases here
                _ => throw new NotSupportedException($"Mailer settings of type '{typeof(T).Name}' are not supported.")
            };
        }
    }
}
