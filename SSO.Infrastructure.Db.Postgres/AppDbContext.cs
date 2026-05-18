using Microsoft.EntityFrameworkCore;
using SSO.Infrastructure.Configs;
using SSO.Infrastructure.Enums;

namespace SSO.Infrastructure.Db.Postgres
{
    public class AppDbContext : AppDbContextBase
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ApplicationConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new ApplicationPermissionConfig());
            builder.ApplyConfiguration(new ApplicationRoleConfig());
            builder.ApplyConfiguration(new ApplicationUserConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new ApplicationRoleClaimConfig());
            builder.ApplyConfiguration(new ApplicationCallbackConfig());
            builder.ApplyConfiguration(new GroupConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new GroupUserConfig());
            builder.ApplyConfiguration(new GroupRoleConfig());
            builder.ApplyConfiguration(new RealmConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new RealmUserConfig());
            builder.ApplyConfiguration(new RealmIdpSettingsConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new RealmMailerSettingsConfig(DatabaseType.Postgres));
            builder.ApplyConfiguration(new RootUserConfig());

            base.OnModelCreating(builder);
        }
    }
}
