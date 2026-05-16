using MediatR;
using SSO.Business.Captchas;

namespace SSO.Business.Accounts.Commands
{
    public class PasswordRecoveryCommand : IRequest<Unit>
    {
        public CaptchaRequest Captcha { get; set; }
        public string Email { get; set; }
        public Guid? RealmId { get; set; }
    }
}
