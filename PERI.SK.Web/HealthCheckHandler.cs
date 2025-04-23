using System.Globalization;
using PERI.SK.Domain.Interfaces;
using PERI.SK.Infrastructure.Data;
using PERI.SK.Infrastructure.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PERI.SK.Web
{
    public class HealthCheckHandler : IHealthCheck
    {
        readonly IConfiguration _configuration;
        readonly IServiceProvider _serviceProvider;
        readonly IReferenceDataService _referenceDataService;

        public HealthCheckHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _referenceDataService = serviceProvider.GetRequiredService<IReferenceDataService>();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                { "version", $"{typeof(Program).Assembly.GetName().Version}" },
                { "dateTime", new { DateTime.UtcNow, LocalNow = DateTime.Now } },
                { "cultureInfo", CultureInfo.CurrentCulture.Name },
                { "platform", Environment.OSVersion.Platform },
                { "aiModel", _configuration.GetSection("AiPlatform:Model").Value! }
            };

            ProcessSqlSources(ref data);

            ProcessPdfSources(ref data);

            return await Task.FromResult(HealthCheckResult.Healthy(string.Empty, data));
        }

        // TODO: Currently getting all ConnectionStrings, assuming all were SQL
        private void ProcessSqlSources(ref Dictionary<string, object> data)
        {
            var sqlQueries = _serviceProvider.GetRequiredService<SqlQueries>();

            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");

            foreach (var connection in connectionStringsSection.GetChildren())
            {
                var name = connection.Key;
                var connectionString = connection.Value;
                
                data.Add(name, (sqlQueries.CanConnect(connectionString!).Result).ToString());
            }
        }

        private void ProcessPdfSources(ref Dictionary<string, object> data)
        {
            var pdfQueries = _serviceProvider.GetRequiredService<PdfQueries>();
            var references = _referenceDataService.GetReferenceData().Result.Where(x => x.Type == nameof(ReferenceDataType.Pdf));

            foreach (var reference in references)
            {
                var name = $"{reference.Subject}";
                var canConnect = pdfQueries.CanConnect(reference.Reference).Result;

                data.Add(name, canConnect.ToString());
            }
        }
    }
}
