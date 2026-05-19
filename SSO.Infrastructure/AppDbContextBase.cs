using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SSO.Domain.Interfaces;
using SSO.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SSO.Infrastructure
{
    public abstract class AppDbContextBase : IdentityDbContext<ApplicationUser, ApplicationRole, string, ApplicationUserClaim, IdentityUserRole<string>, IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>, IAppDbContext
    {
        protected AppDbContextBase(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ApplicationRole> Roles { get; set; }
        public DbSet<ApplicationUserClaim> UserClaims { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        public DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        public DbSet<ApplicationRoleClaim> RoleClaims { get; set; }
        public DbSet<IdentityUserToken<string>> UserTokens { get; set; }

        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationPermission> ApplicationPermissions { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }
        public DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }
        public DbSet<ApplicationCallback> ApplicationCallbacks { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<GroupRole> GroupRoles { get; set; }
        public DbSet<Realm> Realms { get; set; }
        public DbSet<RealmUser> RealmUsers { get; set; }
        public DbSet<RealmIdpSettings> RealmIdpSettings { get; set; }
        public DbSet<RootUser> RootUsers { get; set; }
        public DbSet<RealmMailerSettings> RealmMailerSettings { get; set; }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => base.SaveChangesAsync(cancellationToken);
        public int SaveChanges() => base.SaveChanges();

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => base.Entry(entity);

        public void Add<TEntity>(TEntity entity) where TEntity : class => base.Add(entity);
        public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class => await base.AddAsync(entity);
        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => base.AddRange(entities);
        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => base.AddRangeAsync(entities);

        public void Remove<TEntity>(TEntity entity) where TEntity : class => base.Remove(entity);
        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => base.RemoveRange(entities);

    }
}
