using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SSO.Infrastructure.Mailer
{
    public static class ServiceCollection
    {
        public static void ApplyMailerServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<MailerService>();
        }
    }
}
