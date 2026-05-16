using System.Text.Json.Serialization;

namespace SSO.Infrastructure.Mailer.Dtos
{
    public class SmtpDto
    {
        [JsonPropertyName("smtpServer")]
        public string SmtpServer { get; set; }

        [JsonPropertyName("port")]
        public short Port { get; set; }

        [JsonPropertyName("enableSsl")]
        public bool EnableSsl { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
