using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Domain.Models;
using SSO.Infrastructure.Enums;

namespace SSO.Infrastructure.Configs
{
    public class RealmMailerSettingsConfig : IEntityTypeConfiguration<RealmMailerSettings>
    {
        readonly DatabaseType _dbType;

        public RealmMailerSettingsConfig(DatabaseType? dbType = DatabaseType.SqlServer)
        {
            _dbType = dbType.Value;
        }

        public void Configure(EntityTypeBuilder<RealmMailerSettings> builder)
        {
            builder.HasKey(x => x.RealmId);

            (_dbType switch
            {
                DatabaseType.MySql => (Action<EntityTypeBuilder<RealmMailerSettings>>)UseMySql,
                DatabaseType.Postgres => UsePostgres,
                _ => UseSqlServer
            })(builder);
        }

        void UseSqlServer(EntityTypeBuilder<RealmMailerSettings> builder)
        {
            builder.Property(x => x.MailerType).HasColumnType("tinyint");
        }

        void UseMySql(EntityTypeBuilder<RealmMailerSettings> builder)
        {
            builder.Property(x => x.MailerType).HasColumnType("tinyint");
        }

        void UsePostgres(EntityTypeBuilder<RealmMailerSettings> builder)
        {
            builder.Property(x => x.MailerType).HasColumnType("smallint");
        }
    }
}
