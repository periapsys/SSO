using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Captchas.Queries;

namespace SSO.Business.Captchas.Handlers
{
    public class ValidateCaptchaQueryHandler : IRequestHandler<ValidateCaptchaQuery, Unit>
    {
        readonly IMemoryCache _cache;

        public ValidateCaptchaQueryHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<Unit> Handle(ValidateCaptchaQuery request, CancellationToken cancellationToken)
        {
            if (!_cache.TryGetValue($"captcha:{request.Captcha.Id}", out string? answer))
                throw new ArgumentException("Captcha not found or expired.");

            if (answer != request.Captcha.Answer)
                throw new ArgumentException("Incorrect captcha answer.");

            return new();
        }
    }
}
