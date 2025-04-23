namespace PERI.SK.Domain.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Starts a converstation
        /// </summary>
        /// <param name="query"></param>
        /// <param name="requestor"></param>
        /// <returns>Message</returns>
        Task<string> Converse(string query, string requestor);

        /// <summary>
        /// Gets the message from prompts.json
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetPrompt(string key);

        /// <summary>
        /// Gets the message from responses.json
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetResponse(string key);

        /// <summary>
        /// Gets the topics
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetSubjects();
    }
}
