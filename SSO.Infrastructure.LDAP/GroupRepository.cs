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
    public class GroupRepository : GroupRepositoryBase, IDisposable
    {
        readonly IAppDbContext _context;
        readonly JwtSecretService _jwtSecretService;
        readonly RsaKeyService _rsaKeyService;
        private bool _disposed = false;
        private DirectoryEntry _dirEntry;
        private DirectorySearcher _dirSearcher;

        public GroupRepository(IAppDbContext context, JwtSecretService jwtSecretService, RsaKeyService rsaKeyService) : base(context)
        {
            _context = context;
            _jwtSecretService = jwtSecretService;
            _rsaKeyService = rsaKeyService;
        }

        public override async Task<Group> Add(Group param, bool? saveChanges = true, object? args = null)
        {
            (_dirEntry, _dirSearcher) = await GetLdapConnectionAsync(param.RealmId);

            var newGroup = _dirEntry.Children.Add($"CN={param.Name}", "group");

            newGroup.Properties["sAMAccountName"].Value = param.Name;

            if (!string.IsNullOrEmpty(param.Description))
                newGroup.Properties["description"].Value = param.Description;

            newGroup.CommitChanges();

            await _context.AddAsync(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return param;
        }

        public override async Task Delete(Group param, bool? saveChanges = true, object? args = null)
        {
            (_dirEntry, _dirSearcher) = await GetLdapConnectionAsync(param.RealmId);

            _dirSearcher.Filter = $"(&(objectClass=group)(sAMAccountName={param.Name}))";
            _dirSearcher.SearchScope = SearchScope.Subtree;

            var result = _dirSearcher.FindOne();

            if (result != null)
            {
                var userEntry = result.GetDirectoryEntry();
                userEntry.DeleteTree();
                _dirEntry.CommitChanges();
            }

            _context.Remove(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();
        }

        public override async Task<Group> Update(Group param, bool? saveChanges = true, object? args = null)
        {
            (_dirEntry, _dirSearcher) = await GetLdapConnectionAsync(param.RealmId);

            var rec = await _context.Groups.FirstAsync(x => x.GroupId == param.GroupId);

            _dirSearcher.Filter = $"(&(objectClass=group)(sAMAccountName={rec.Name}))";
            _dirSearcher.SearchScope = SearchScope.Subtree;

            var result = _dirSearcher.FindOne();

            if (result != null)
            {
                var groupEntry = result.GetDirectoryEntry();

                groupEntry.Rename($"CN={param.Name}");
                groupEntry.Properties["sAMAccountName"].Value = param.Name;
                groupEntry.Properties["description"].Value = param.Description ?? string.Empty;
                groupEntry.CommitChanges();
            }

            rec.Name = param.Name;
            rec.Description = param.Description;
            rec.ModifiedBy = param.ModifiedBy;
            rec.DateModified = DateTime.Now;

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return rec;
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

        private async Task<(DirectoryEntry dirEntry, DirectorySearcher dirSearcher)> GetLdapConnectionAsync(Guid realmId)
        {
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
