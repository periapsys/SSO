using System.Text.RegularExpressions;
using PERI.SK.Domain.Models;
using PERI.SK.Infrastructure.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace PERI.SK.Infrastructure
{
    public partial class ChatService
    {
        /// <summary>
        /// Processes SQL ChatCompletion
        /// </summary>
        /// <param name="refData"></param>
        /// <param name="chatHistory"></param>
        /// <param name="connectionString"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<string> ProcessSql(ReferenceData refData, ChatHistory chatHistory, string connectionString, string? query = null)
        {
            var sqlQueries = _serviceProvider.GetRequiredService<SqlQueries>();

            var subject = refData.Subject;
            var schema = refData.Reference.Split('.')[0];
            var table = refData.Reference.Split('.')[1];

            var cacheKey = $"{subject}_{schema}_{table}_{nameof(sqlQueries.GetFields)}";
            var fields = _cache.Get<string>(cacheKey) ?? await sqlQueries.GetFields(connectionString!, schema, table);            
            _cache.Set(cacheKey, fields, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
            var prompt = string.Format(await GetPrompt("generate_sql"), fields, query);

            chatHistory.AddUserMessage(prompt);
            var request = await GetChatResponse(chatHistory);
            chatHistory.AddAssistantMessage(request);

            var pattern = @"```sql(.*?)```";
            var match = Regex.Match(request.ToString(), pattern, RegexOptions.Singleline);
            string data;

            try
            {
                if (match.Success)
                {
                    var sql = match.Groups[1].Value;
                    data = await sqlQueries.GetData(connectionString!, sql.Trim().Replace("\n", " "));
                }
                else
                    data = await sqlQueries.GetData(connectionString!, request);

                if (string.IsNullOrEmpty(data))
                    return await GetResponse("no_result");

                prompt = string.Format(await GetPrompt("make_data_readable"), data, fields);
                chatHistory.AddUserMessage(prompt);
                request = await GetChatResponse(chatHistory);
                chatHistory.AddAssistantMessage(request);

                return request;
            }
            catch
            {
                return "Unable to process your query.";
            }
        }
    }
}
