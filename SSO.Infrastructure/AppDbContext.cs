﻿using Microsoft.EntityFrameworkCore;
using SSO.Infrastructure.Configs;

namespace SSO.Infrastructure
{
    public class AppDbContext : AppDbContextBase
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ApplicationConfig());
            builder.ApplyConfiguration(new ApplicationPermissionConfig());
            builder.ApplyConfiguration(new ApplicationRoleConfig());
            builder.ApplyConfiguration(new ApplicationUserConfig());
            builder.ApplyConfiguration(new ApplicationRoleClaimConfig());
            builder.ApplyConfiguration(new ApplicationCallbackConfig());
            builder.ApplyConfiguration(new GroupConfig());
            builder.ApplyConfiguration(new GroupUserConfig());
            builder.ApplyConfiguration(new GroupRoleConfig());
            builder.ApplyConfiguration(new RealmConfig());
            builder.ApplyConfiguration(new RealmUserConfig());

            base.OnModelCreating(builder);
        }
    }
}
