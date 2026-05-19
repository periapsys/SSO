using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SSO.Business.Accounts.Commands;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;
using SSO.Infrastructure.LDAP.Models;
using SSO.Infrastructure.Settings.Enums;
using System.DirectoryServices;

namespace SSO.Business.Accounts.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        readonly IRealmRepository _realmRepository;
        readonly UserManager<ApplicationUser> _userManager;
        readonly Users.RepositoryFactory _userRepoFactory;

        public ChangePasswordCommandHandler(IRealmRepository realmRepository, UserManager<ApplicationUser> userManager, Users.RepositoryFactory userRepoFactory)
        {
            _realmRepository = realmRepository;
            _userManager = userManager;
            _userRepoFactory = userRepoFactory;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var realm = await _realmRepository.FindOne(x => x.RealmId == request.RealmId);
            var isLdap = realm.IdpSettingsCollection.Any(x => x.IdentityProvider == IdentityProvider.LDAP);

            var userRepo = await _userRepoFactory.GetRepository(request.RealmId);

            request.User = await userRepo.FindOne(x => x.Id == request.User!.Id);

            if (isLdap)
                await ValidateLdapPassword(request, realm);
            else
                if (!(await _userManager.CheckPasswordAsync(request.User, request.CurrentPassword)))
                    throw new ArgumentException(message: "Incorrect password.", paramName: "InvalidPassword");

            await userRepo.ChangePassword(request.User, request.NewPassword, request.Author, default);

            return new Unit();
        }

        private async Task ValidateLdapPassword(ChangePasswordCommand request, Realm realm)
        {
            try
            {
                var idpSettings = realm.IdpSettingsCollection.FirstOrDefault(x => x.IdentityProvider == IdentityProvider.LDAP) ?? throw new ArgumentException("LDAP is not configured.");
                var ldapSettings = JsonConvert.DeserializeObject<LDAPSettings>(idpSettings.Value);
                var ldapConnectionString = $"{(ldapSettings.UseSSL ? "LDAPS" : "LDAP")}://{ldapSettings.Server}:{ldapSettings.Port}/{ldapSettings.SearchBase}";

                using (DirectoryEntry entry = new DirectoryEntry(ldapConnectionString, request.User.UserName, request.CurrentPassword, AuthenticationTypes.Secure))
                {
                    // Attempt to bind to the directory using the provided credentials
                    object nativeObject = entry.NativeObject;

                    // If no exception is thrown, the credentials are valid
                    Console.WriteLine($"Success!");
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(message: "Incorrect password.");
            }
        }
    }
}
