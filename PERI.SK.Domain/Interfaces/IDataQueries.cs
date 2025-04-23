namespace PERI.SK.Domain.Interfaces
{
    public interface IDataQueries
    {
        /// <summary>
        /// Gets the necessary fields to form the schema
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Task<string> GetFields(string connectionString, string schema, string table);

        /// <summary>
        /// Gets the data result as string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        Task<string> GetData(string connectionString, string? query = null);

        /// <summary>
        /// Checks whether the connection is reachable
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        Task<bool> CanConnect(string connectionString);
    }
}
