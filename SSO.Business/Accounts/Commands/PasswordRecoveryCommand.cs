using MediatR;
using SSO.Business.Captchas;
using System.Text.Json.Serialization;

namespace SSO.Business.Accounts.Commands
{
    public class PasswordRecoveryCommand : IRequest<Unit>
    {
        public CaptchaRequest Captcha { get; set; }
        public string Email { get; set; }

        [JsonIgnore]
        public Guid? RealmId { get; set; }

        [JsonIgnore]
        public string? BaseUrl { get; set; }
    }
}
