using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Accounts.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;

namespace SSO.Business.Accounts.Handlers
{
    public class PasswordRecoveryCommandHandler : IRequestHandler<PasswordRecoveryCommand, Unit>
    {
        readonly IMemoryCache _cache;
        readonly IRealmRepository _realmRepository;
        readonly UserManager<ApplicationUser> _userManager;

        public PasswordRecoveryCommandHandler(IMemoryCache cache, IRealmRepository realmRepository, UserManager<ApplicationUser> userManager)
        {
            _cache = cache;
            _realmRepository = realmRepository;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(PasswordRecoveryCommand request, CancellationToken cancellationToken)
        {
            if (!_cache.TryGetValue($"captcha:{request.Captcha.Id}", out string? answer))
                throw new ArgumentException("Captcha not found or expired.");

            if (answer != request.Captcha.Answer)
                throw new ArgumentException("Incorrect captcha answer.");

            var realm = await _realmRepository.FindOne(x => x.RealmId == request.RealmId);
            realm ??= await _realmRepository.FindOne(x => x.Name == "root");

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new ArgumentException("User with the provided email does not exist.");

            return new();
        }
    }
}
