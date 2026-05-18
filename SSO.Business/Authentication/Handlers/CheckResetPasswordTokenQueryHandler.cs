using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Authentication.Queries;

namespace SSO.Business.Authentication.Handlers
{
    public class CheckResetPasswordTokenQueryHandler : IRequestHandler<CheckResetPasswordTokenQuery, Unit>
    {
        readonly IMemoryCache _cache;

        public CheckResetPasswordTokenQueryHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<Unit> Handle(CheckResetPasswordTokenQuery request, CancellationToken cancellationToken)
        {
            var key = $"recovery:{request.Token}";

            if (!_cache.TryGetValue(key, out string? token))
                throw new ArgumentException("Token not found or expired.");

            return new();
        }
    }
}
