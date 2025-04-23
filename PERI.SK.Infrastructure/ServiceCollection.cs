using PERI.SK.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PERI.SK.Infrastructure
{
    public static class ServiceCollection
    {
        public static void ApplyInfrastructureServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IReferenceDataService, ReferenceDataService>();
        }
    }
}
