#pragma warning disable SKEXP0010

using PERI.SK.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace PERI.SK.Infrastructure.DeepSeek
{
    public static class ServiceCollection
    {
        public static void ApplyDeepSeekServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            var model = configuration.GetSection("AiPlatform:Model").Value;
            var endpoint = configuration.GetSection("AiPlatform:Endpoint").Value;
            var apiKey = configuration.GetSection("AiPlatform:ApiKey").Value;

            services.AddSingleton<Kernel>(serviceProvider =>
            {
                var builder = Kernel.CreateBuilder();
                builder.AddOpenAIChatCompletion(model!, new Uri(endpoint!), apiKey!);
                builder.AddLocalTextEmbeddingGeneration();
                return builder.Build();
            });

            services.AddScoped<IChatService, ChatService>();
        }
    }
}
