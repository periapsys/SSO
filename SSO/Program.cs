using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OwaspHeaders.Core.Enums;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using SSO;
using SSO.Business;
using SSO.Business.Authentication.Handlers;
using SSO.Business.Mappings;
using SSO.Domain.Management.Interfaces;
using SSO.Filters;
using SSO.Infrastructure;
using SSO.Infrastructure.Db.MySql;
using SSO.Infrastructure.Db.Postgres;
using SSO.Infrastructure.LDAP;
using SSO.Infrastructure.Management;
using SSO.Infrastructure.Settings.Constants;
using SSO.Infrastructure.Settings.Services;
using System.Data.Common;
using System.Reflection;
using System.Security.Claims;
using VueCliMiddleware;

var builder = WebApplication.CreateBuilder(args);

var modelBuilder = ODataModelBuilderFactory.Create();

builder.Services.AddControllers(options => {
    options.Filters.Add(new GlobalExceptionFilter());
}).AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionStringBuilder = new DbConnectionStringBuilder { ConnectionString = connectionString };
var keys = connectionStringBuilder.Keys.Cast<string>().ToList();

if (keys.Any(k => k.Equals("Uid", StringComparison.OrdinalIgnoreCase)))
    builder.Services.ApplyMySqlServiceCollection(builder.Configuration);
else if (keys.Any(k => k.Equals("Host", StringComparison.OrdinalIgnoreCase)))
    builder.Services.ApplyPostgresServiceCollection(builder.Configuration);
else
    builder.Services.ApplySqlServerServiceCollection(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(ApplicationProfile).Assembly);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginQueryHandler>());

var rsaKeyService = new RsaKeyService();
var pemFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private_key.pem");
var privateKey = rsaKeyService.CreatePrivateKey(pemFilePath);
var publicKey = rsaKeyService.CreatePublicKey(privateKey, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public_key.pem"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = TokenValidationParamConstants.Audience,
        ValidIssuer = TokenValidationParamConstants.Issuer,
        IssuerSigningKey = new RsaSecurityKey(publicKey)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("RealmAccessPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("realm");
        policy.RequireClaim(ClaimTypes.Role);
    });
});

builder.Services.AddSingleton(_ => new JwtSecretService(privateKey));
builder.Services.AddSingleton(_ => rsaKeyService);

builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IApplicationRoleRepository, ApplicationRoleRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IApplicationCallbackRepository, ApplicationCallbackRepository>();
builder.Services.AddScoped<IApplicationPermissionRepository, ApplicationPermissionRepository>();
builder.Services.AddScoped<IApplicationRoleClaimRepository, ApplicationRoleClaimRepository>();
builder.Services.AddScoped<IGroupRoleRepository, GroupRoleRepository>();
builder.Services.AddScoped<IRealmRepository, RealmRepository>();
builder.Services.AddScoped<IRealmUserRepository, RealmUserRepository>();
builder.Services.AddScoped<IRealmIdpSettingsRepository, RealmIdpSettingsRepository>();

builder.Services.ApplyBusinessServiceCollection(builder.Configuration);
builder.Services.ApplyLdapServiceCollection(builder.Configuration);

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist";
});

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("Client", new OpenApiInfo { Title = "Client", Version = $"v{typeof(Program).Assembly.GetName().Version}" });
    x.SwaggerDoc("System", new OpenApiInfo { Title = "System", Version = $"v{typeof(Program).Assembly.GetName().Version}" });
    x.SwaggerDoc("Root", new OpenApiInfo { Title = "Root", Version = $"v{typeof(Program).Assembly.GetName().Version}" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    x.IncludeXmlComments(xmlPath);

    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAnyOrigin", policy => {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks().AddCheck<HealthCheckHandler>(nameof(HealthCheckHandler));

var app = builder.Build();

// Obtain an instance of IWebHostEnvironment through dependency injection
var env = app.Services.GetRequiredService<IWebHostEnvironment>();

// Configure the HTTP request pipeline.

#if DEBUG
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
#else
app.UseCors();
#endif

app.UseSecureHeadersMiddleware(CustomConfiguration());
app.UseSpaStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

string[] prefixes = { "/swagger", "/api", "/odata", "/hangfire", "/connect", "/.well-known" };

app.MapWhen(r => !prefixes.Any(p => r.Request.Path.Value.StartsWith(p)), builder =>
{
    builder.UseSpa(spa =>
    {
        if (env.IsDevelopment())
        {
            spa.Options.SourcePath = "ClientApp/";
            spa.UseVueCli(npmScript: "serve");
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/Client/swagger.json", "Client");
    options.SwaggerEndpoint("/swagger/System/swagger.json", "System");
    options.SwaggerEndpoint("/swagger/Root/swagger.json", "Root");
});

#pragma warning disable CS0618 // Type or member is obsolete
app.UseHangfireServer();
#pragma warning restore CS0618 // Type or member is obsolete
app.UseHangfireDashboard();

app.Run();

SecureHeadersMiddlewareConfiguration CustomConfiguration()
{
    return SecureHeadersMiddlewareBuilder
        .CreateBuilder()
        .UseHsts(1200, false)
        .UseContentSecurityPolicy(
            blockAllMixedContent: false,
            upgradeInsecureRequests: false)
        .UsePermittedCrossDomainPolicies(XPermittedCrossDomainOptionValue.masterOnly)
        .UseReferrerPolicy(ReferrerPolicyOptions.sameOrigin)
        .UseXFrameOptions()
        .Build();
}
