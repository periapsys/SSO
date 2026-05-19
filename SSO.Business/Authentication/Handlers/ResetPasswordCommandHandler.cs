using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Authentication.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;

namespace SSO.Business.Authentication.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        readonly IMemoryCache _cache;
        readonly IRealmRepository _realmRepository;
        readonly UserManager<ApplicationUser> _userManager;
        readonly Users.RepositoryFactory _userRepoFactory;

        public ResetPasswordCommandHandler(IMemoryCache cache,
            IRealmRepository realmRepository,
            UserManager<ApplicationUser> userManager,
            Users.RepositoryFactory userRepoFactory)
        {
            _cache = cache;
            _realmRepository = realmRepository;
            _userManager = userManager;
            _userRepoFactory = userRepoFactory;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var key = $"recovery:{request.Token}";

            if (!_cache.TryGetValue(key, out string? token))
                throw new ArgumentException("Token not found or expired.");

            var realm = await _realmRepository.FindOne(x => x.RealmId == request.RealmId);
            var userRepo = await _userRepoFactory.GetRepository(request.RealmId);
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            await userRepo.ChangePassword(user, request.NewPassword, default, new { Token = token });

            _cache.Remove(key);

            return new();
        }
    }
}
