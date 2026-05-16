using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SSO.Domain.Interfaces
{
    public interface IAppDbContext
    {
        // DbSet properties for Identity entities
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<ApplicationRole> Roles { get; set; }
        DbSet<ApplicationUserClaim> UserClaims { get; set; }
        DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        DbSet<ApplicationRoleClaim> RoleClaims { get; set; }
        DbSet<IdentityUserToken<string>> UserTokens { get; set; }

        // DbSet properties for additional entities
        DbSet<Application> Applications { get; set; }
        DbSet<ApplicationPermission> ApplicationPermissions { get; set; }
        DbSet<ApplicationRole> ApplicationRoles { get; set; }
        DbSet<ApplicationUser> ApplicationUsers { get; set; }
        DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }
        DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }
        DbSet<ApplicationCallback> ApplicationCallbacks { get; set; }
        DbSet<Group> Groups { get; set; }
        DbSet<GroupUser> GroupUsers { get; set; }
        DbSet<GroupRole> GroupRoles { get; set; }
        DbSet<Realm> Realms { get; set; }
        DbSet<RealmUser> RealmUsers { get; set; }
        DbSet<RealmIdpSettings> RealmIdpSettings { get; set; }
        DbSet<RealmMailerSettings> RealmMailerSettings { get; set; }
        DbSet<RootUser> RootUsers { get; set; }

        // Database property from DbContext
        DatabaseFacade Database { get; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        // Method signatures for saving changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();

        // Default EF functions
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        void Add<TEntity>(TEntity entity) where TEntity : class;
        Task AddAsync<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
