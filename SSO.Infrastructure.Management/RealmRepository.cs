using Microsoft.EntityFrameworkCore;
using SSO.Domain.Interfaces;
using SSO.Domain.Management.Interfaces;
using SSO.Domain.Models;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Management
{
    public class RealmRepository : IRealmRepository
    {
        readonly IAppDbContext _context;

        public RealmRepository(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Realm> Add(Realm param, bool? saveChanges = true, object? args = null)
        {
            _context.Add(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return param;
        }

        public async Task<bool> Any(Expression<Func<Realm, bool>> predicate)
        {
            return await _context.Realms.AnyAsync(predicate);
        }

        public async Task Delete(Realm param, bool? saveChanges = true, object? args = null)
        {
            _context.Remove(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<Realm>> Find(Expression<Func<Realm, bool>>? predicate)
        {
            if (predicate is not null)
                return await Task.Run(() => _context.Realms.Where(predicate).AsQueryable().AsNoTracking());
            else
                return _context.Realms.AsQueryable().AsNoTracking();
        }

        public async Task<Realm> FindOne(Expression<Func<Realm, bool>> predicate)
        {
            return await _context.Realms
                .Include(x => x.IdpSettingsCollection)
                .Include(x => x.RealmMailerSettings)
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<Realm> Update(Realm param, bool? saveChanges = true, object? args = null)
        {
            var rec = await _context.Realms.FirstAsync(x => x.RealmId == param.RealmId);

            _context.Entry(rec).CurrentValues.SetValues(param);

            if (saveChanges!.Value)
                await _context.SaveChangesAsync();

            return rec;
        }
    }
}
