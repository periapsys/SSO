using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SSO.Domain.Interfaces;
using SSO.Domain.Models;
using SSO.Infrastructure.LDAP.Models;
using SSO.Infrastructure.Management;
using SSO.Infrastructure.Settings.Enums;
using SSO.Infrastructure.Settings.Services;
using System.DirectoryServices;

namespace SSO.Infrastructure.LDAP
{
    public class GroupUserRepository : GroupUserRepositoryBase, IDisposable
    {
        readonly IAppDbContext _context;
        readonly JwtSecretService _jwtSecretService;
        readonly RsaKeyService _rsaKeyService;
        private DirectoryEntry _dirEntry;
        private DirectorySearcher _dirSearcher;
        private bool _disposed;

        public GroupUserRepository(IAppDbContext context, JwtSecretService jwtSecretService, RsaKeyService rsaKeyService) : base(context)
        {
            _context = context;
            _jwtSecretService = jwtSecretService;
            _rsaKeyService = rsaKeyService;
        }

        public override Task<GroupUser> Add(GroupUser param, bool? saveChanges = true, object? args = null)
        {
            var (userEntry, groupEntry) = FindUserAndGroupEntries(param);

            if (userEntry != null && groupEntry != null)
            {
                AddUserToGroup(userEntry, groupEntry);
                groupEntry.CommitChanges();
            }

            _context.Add(param);
            if (saveChanges!.Value)
                return _context.SaveChangesAsync().ContinueWith(_ => param);

            return Task.FromResult(param);
        }

        public override Task Delete(GroupUser param, bool? saveChanges = true, object? args = null)
        {
            var (userEntry, groupEntry) = FindUserAndGroupEntries(param);

            if (userEntry != null && groupEntry != null)
            {
                RemoveUserFromGroup(userEntry, groupEntry);
                groupEntry.CommitChanges();
            }

            _context.Remove(param);

            if (saveChanges!.Value)
                return _context.SaveChangesAsync();

            return Task.CompletedTask;
        }

        private (DirectoryEntry?, DirectoryEntry?) FindUserAndGroupEntries(GroupUser param)
        {
            (_dirEntry, _dirSearcher) = GetLdapConnectionAsync(param).Result;

            var group = _context.Groups.First(x => x.GroupId == param.GroupId).Name;
            var userName = _context.Users.First(x => x.Id == param.UserId).UserName;

            var userEntry = FindDirectoryEntryByFilter($"(&(objectClass=user)(sAMAccountName={userName}))");
            var groupEntry = FindDirectoryEntryByFilter($"(&(objectClass=group)(sAMAccountName={group}))");

            return (userEntry, groupEntry);
        }

        private DirectoryEntry? FindDirectoryEntryByFilter(string filter)
        {
            _dirSearcher.Filter = filter;
            var searchResult = _dirSearcher.FindOne();
            return searchResult?.GetDirectoryEntry();
        }

        private void AddUserToGroup(DirectoryEntry userEntry, DirectoryEntry groupEntry)
        {
            groupEntry.Properties["member"].Add(userEntry.Properties["distinguishedName"].Value);
        }

        private void RemoveUserFromGroup(DirectoryEntry userEntry, DirectoryEntry groupEntry)
        {
            groupEntry.Properties["member"].Remove(userEntry.Properties["distinguishedName"].Value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dirSearcher?.Dispose();
                    _dirEntry?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task<(DirectoryEntry dirEntry, DirectorySearcher dirSearcher)> GetLdapConnectionAsync(GroupUser groupUser)
        {
            var realmId = (await _context.Groups.FirstAsync(x => x.GroupId == groupUser.GroupId)).RealmId;
            var realm = await _context.Realms.Include(x => x.IdpSettingsCollection).FirstAsync(x => x.RealmId == realmId);
            var idpSettings = realm.IdpSettingsCollection.FirstOrDefault(x => x.IdentityProvider == IdentityProvider.LDAP) ?? throw new ArgumentException("LDAP is not configured.");
            var decStr = _rsaKeyService.DecryptString(idpSettings.Value, _jwtSecretService.PrivateKey);
            var ldapSettings = JsonConvert.DeserializeObject<LDAPSettings>(decStr);
            var ldapConnectionString = $"{(ldapSettings.UseSSL ? "LDAPS" : "LDAP")}://{ldapSettings.Server}:{ldapSettings.Port}/{ldapSettings.SearchBase}";
            _dirEntry = new DirectoryEntry(ldapConnectionString, ldapSettings.Username, ldapSettings.Password);
            _dirSearcher = new DirectorySearcher(_dirEntry);

            return (_dirEntry, _dirSearcher);
        }
    }
}
