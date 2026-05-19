using MediatR;
using Newtonsoft.Json.Linq;
using SSO.Business.Versions.Queries;

namespace SSO.Business.Versions.Handlers
{
    public class GetLatestVersionQueryHandler : IRequestHandler<GetLatestVersionQuery, string>
    {
        public async Task<string> Handle(GetLatestVersionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MyApp/1.0)");

                var response = await client.GetStringAsync("https://api.github.com/repos/periapsys/sso/releases");
                var array = JArray.Parse(response);
                var firstItem = (JObject)array[0];

                return firstItem["tag_name"]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
