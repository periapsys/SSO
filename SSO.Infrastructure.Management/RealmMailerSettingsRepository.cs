using Microsoft.EntityFrameworkCore;
using SSO.Domain.Interfaces;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Management
{
    public class RealmMailerSettingsRepository : IRealmMailerSettingsRepository
    {
        readonly IAppDbContext _context;

        public RealmMailerSettingsRepository(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RealmMailerSettings> Add(RealmMailerSettings param, bool? saveChanges = true, object? args = null)
        {
            _context.Add(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return param;
        }

        public Task<bool> Any(Expression<Func<RealmMailerSettings, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(RealmMailerSettings param, bool? saveChanges = true, object? args = null)
        {
            _context.Remove(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<RealmMailerSettings>> Find(Expression<Func<RealmMailerSettings, bool>>? predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<RealmMailerSettings> FindOne(Expression<Func<RealmMailerSettings, bool>> predicate)
        {
            return await _context.RealmMailerSettings.FirstOrDefaultAsync(predicate);
        }

        public async Task<RealmMailerSettings> Update(RealmMailerSettings param, bool? saveChanges = true, object? args = null)
        {
            var rec = await _context.RealmMailerSettings.FirstAsync(x => x.RealmId == param.RealmId);

            _context.Entry(rec).CurrentValues.SetValues(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return rec;
        }
    }
}
