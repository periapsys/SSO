using MediatR;
using Microsoft.AspNetCore.Identity;
using SSO.Business.Accounts.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;

namespace SSO.Business.Accounts.Handlers
{
    public class PasswordRecoveryCommandHandler : IRequestHandler<PasswordRecoveryCommand, Unit>
    {
        readonly IRealmRepository _realmRepository;
        readonly UserManager<ApplicationUser> _userManager;

        public PasswordRecoveryCommandHandler(IRealmRepository realmRepository, UserManager<ApplicationUser> userManager)
        {
            _realmRepository = realmRepository;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(PasswordRecoveryCommand request, CancellationToken cancellationToken)
        {
            var realm = await _realmRepository.FindOne(x => x.RealmId == request.RealmId);
            realm ??= await _realmRepository.FindOne(x => x.Name == "root");

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new ArgumentException("User with the provided email does not exist.");

            return new();
        }
    }
}
