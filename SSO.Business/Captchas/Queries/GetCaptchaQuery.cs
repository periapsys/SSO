using MediatR;

namespace SSO.Business.Captchas.Queries
{
    public class GetCaptchaQuery : IRequest<GetCaptchaResult>
    {
        public Guid? Id { get; set; }
    }
}
