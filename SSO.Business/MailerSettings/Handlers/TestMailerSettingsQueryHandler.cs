using MediatR;
using SSO.Business.MailerSettings.Queries;
using SSO.Infrastructure.Mailer;
using SSO.Infrastructure.Mailer.Dtos;
using SSO.Infrastructure.Mailer.Enums;
using System.Text.Json;

namespace SSO.Business.MailerSettings.Handlers
{
    public partial class TestMailerSettingsQueryHandler : IRequestHandler<TestMailerSettingsQuery, Unit>
    {
        readonly MailerService _mailerService;

        public TestMailerSettingsQueryHandler(MailerService mailerService)
        {
            _mailerService = mailerService;
        }

        public async Task<Unit> Handle(TestMailerSettingsQuery request, CancellationToken cancellationToken)
        {
            var jsonElement = (JsonElement)request.Settings;

            (object settings, string username) parameters = request.Type switch
            {
                MailerType.Smtp => GetParameters<SmtpDto>(jsonElement, request.Type),
                _ => throw new NotSupportedException($"MailerType '{request.Type}' is not supported.")
            };

            await _mailerService.SendEmailAsync(request.Type, parameters.settings, parameters.username, "SSO Test", request.ToEmail, "Test Email", "This is a test email to verify the mailer settings.");
            
            return new();
        }

        private (T settings, string username) GetParameters<T>(JsonElement jsonElement, MailerType type) where T : class
        {
            var obj = jsonElement.Deserialize<T>();

            return obj switch
            {
                SmtpDto s => (settings: obj, username: s.Username),
                // TODO: Add other DTO cases here
                _ => throw new NotSupportedException($"Mailer settings of type '{typeof(T).Name}' are not supported.")
            };
        }
    }
}
