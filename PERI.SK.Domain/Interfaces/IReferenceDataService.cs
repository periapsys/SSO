using PERI.SK.Domain.Models;

namespace PERI.SK.Domain.Interfaces
{
    public interface IReferenceDataService
    {
        // TODO: Might expose risk since this also returns ConnectionString
        /// <summary>
        /// Gets ReferenceData object that contains the connection details
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task<(ReferenceData ReferenceData, string? Connectionstring)> GetReferenceData(string subject);

        /// <summary>
        /// Gets all ReferenceData from appsettings
        /// </summary>
        /// <returns></returns>
        Task<List<ReferenceData>> GetReferenceData();
    }
}
