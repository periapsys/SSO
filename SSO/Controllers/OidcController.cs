using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SSO.Controllers
{
    [ApiController]
    public class OidcController : ControllerBase
    {
        [HttpGet]
        [Route(".well-known/openid-configuration")]
        public IActionResult Discovery()
        {
            return Ok(new
            {
                issuer = $"{Request.Scheme}://{Request.Host}",
                authorization_endpoint = $"{Request.Scheme}://{Request.Host}/connect/authorize",
                token_endpoint = $"{Request.Scheme}://{Request.Host}/connect/token",
                userinfo_endpoint = $"{Request.Scheme}://{Request.Host}/connect/userinfo",
                response_types_supported = new[] { "code" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { "none" }
            });
        }

        [HttpGet("connect/authorize")]
        public IActionResult AuthorizeEndpoint()
        {
            var redirectUri = Request.Query["redirect_uri"].ToString();
            var state = Request.Query["state"].ToString();
            var code = "FAKE_AUTH_CODE_123";

            var redirect = $"{redirectUri}?code={code}&state={state}";
            return Redirect(redirect);
        }

        [HttpPost("connect/token")]
        public IActionResult TokenEndpoint()
        {
            // Read form fields directly from Request.Form (no models)
            var form = Request.HasFormContentType ? Request.Form : null;

            var issuer = $"{Request.Scheme}://{Request.Host}";

            // Build unsigned ID token inline (alg: none)
            string headerJson = "{\"alg\":\"none\"}";
            string payloadJson = JsonSerializer.Serialize(new
            {
                sub = "123",
                name = "Fake User123",
                email = "fake@example.com",
                iss = issuer,
                aud = "fake_client",
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            string header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            string payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            string idToken = $"{header}.{payload}.";

            return new JsonResult(new
            {
                access_token = "FAKE_ACCESS_TOKEN_ABC",
                token_type = "Bearer",
                expires_in = 3600,
                id_token = idToken
            });
        }

        [HttpGet("connect/userinfo")]
        public IActionResult UserInfo()
        {
            return Ok(new
            {
                sub = "123",
                name = "Fake User",
                email = "fake@example.com"
            });
        }
    }
}
