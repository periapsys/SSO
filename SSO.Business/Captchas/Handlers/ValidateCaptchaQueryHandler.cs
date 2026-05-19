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
            var key = $"captcha:{request.Captcha.Id}";

            if (!_cache.TryGetValue(key, out string? answer))
                throw new ArgumentException("Captcha not found or expired.");

            _cache.Remove(key);

            if (!string.Equals(answer, request.Captcha.Answer, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Incorrect captcha answer.");

            return new();
        }
    }
}
