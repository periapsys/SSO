using MediatR;

namespace SSO.Business.Captchas.Queries
{
    public class ValidateCaptchaQuery : IRequest<Unit>
    {
        public CaptchaRequest Captcha { get; set; }
    }
}
