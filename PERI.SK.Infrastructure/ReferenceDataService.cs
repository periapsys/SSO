#pragma warning disable CS1998

using PERI.SK.Domain.Interfaces;
using PERI.SK.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace PERI.SK.Infrastructure
{
    public class ReferenceDataService : IReferenceDataService
    {
        readonly IConfiguration _configuration;

        public ReferenceDataService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(ReferenceData ReferenceData, string? Connectionstring)> GetReferenceData(string subject)
        {
            var list = await GetReferenceData();

            // Get the ReferenceData object based on the subject
            var referenceData = list.FirstOrDefault(x => x.Subject.ToLower() == subject.ToLower())
                                ?? throw new ArgumentNullException("Not found.");

            // Get the connection string from the configuration using the referenceData.ConnectionString key
            var connectionString = _configuration.GetConnectionString(referenceData.ConnectionStringName!) ?? referenceData.Reference;

            return (
                ReferenceData: referenceData,
                Connectionstring: connectionString
            );
        }

        public async Task<List<ReferenceData>> GetReferenceData()
        {
            return _configuration.GetSection("ReferenceData").Get<List<ReferenceData>>()
                       ?? throw new ArgumentException("Settings not found.");
        }
    }
}
