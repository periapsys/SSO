using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Captchas.Queries;

namespace SSO.Business.Captchas.Handlers
{
    public class GetCaptchaQueryHandler : IRequestHandler<GetCaptchaQuery, GetCaptchaResult>
    {
        readonly IMemoryCache _cache;

        public GetCaptchaQueryHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<GetCaptchaResult> Handle(GetCaptchaQuery request, CancellationToken cancellationToken)
        {
            var text = Random.Shared.Next(1000, 9999).ToString();
            var id = Guid.NewGuid().ToString();

            _cache.Set($"captcha:{id}", text, TimeSpan.FromMinutes(5));

            var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="120" height="40">
              <rect width="100%" height="100%" fill="#f3f3f3"/>
              <text x="15" y="28"
                    font-size="24"
                    font-family="monospace"
                    fill="#333">{text}</text>
            </svg>
            """;

            return new GetCaptchaResult(id, $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}");
        }
    }
}
