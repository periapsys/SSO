using MediatR;
using Microsoft.AspNetCore.Mvc;
using SSO.Business.Authentication.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SSO.Controllers
{
    [ApiController]
    public class OidcController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OidcController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
        public async Task<IActionResult> Authorize()
        {
            var clientIdRaw = Request.Query["client_id"].ToString();

            if (string.IsNullOrWhiteSpace(clientIdRaw) || !Guid.TryParse(clientIdRaw, out var clientId))
            {
                return BadRequest(new
                {
                    error = "invalid_client",
                    error_description = "client_id must be a valid GUID"
                });
            }

            var redirectUri = Request.Query["redirect_uri"].ToString();
            var state = Request.Query["state"].ToString();

            try
            {
                await _mediator.Send(new InitLoginQuery { ApplicationId = clientId, CallbackUrl = redirectUri });

                Response.Cookies.Append("appId", clientId.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = false });

                if (Request.Cookies["token"] != null)
                {
                    var token = await _mediator.Send(new SwitchAppQuery { Token = Request.Cookies["token"], ApplicationId = clientId });

                    Response.Cookies.Append("token", token.AccessToken, new CookieOptions { Expires = token.Expires, HttpOnly = false });

                    var redirect = $"{redirectUri}?code={token.Id}&state={state}";
                    return Redirect(redirect);
                }

                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={clientId}&callbackUrl={redirectUri}");
            }
            catch (UnauthorizedAccessException)
            {
                // Delete cookie
                Response.Cookies.Delete("token");

                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={clientId}&callbackUrl={redirectUri}");
            }
        }

        [HttpPost("connect/token")]
        public async Task<IActionResult> Token()
        {
            var code = Request.Form["code"];
            var issuer = $"{Request.Scheme}://{Request.Host}";

            var res = await _mediator.Send(new GetAccessTokenQuery { RequestToken = Guid.Parse(code!) });

            // Parse the access token once
            var token = new JwtSecurityTokenHandler().ReadJwtToken(res.AccessToken);
            var givenName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var userId = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            // Inline Base64Url encode helper (no static)
            string Base64UrlEncode(string s) =>
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            // Build unsigned ID token inline
            var header = Base64UrlEncode("{\"alg\":\"none\"}");
            var payload = Base64UrlEncode(JsonSerializer.Serialize(new
            {
                sub = userId,
                name = givenName,
                iss = issuer,
                aud = "SSO",    // TODO: To check
                exp = new DateTimeOffset(res.Expires).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }));
            var idToken = $"{header}.{payload}.";

            return new JsonResult(new
            {
                access_token = res.AccessToken,
                token_type = "Bearer",
                expires_in = (int)(res.Expires - DateTime.UtcNow).TotalSeconds,
                id_token = idToken
            });
        }

        [HttpGet("connect/userinfo")]
        public IActionResult UserInfo()
        {
            // Get the raw access token from the Authorization header
            var accessToken = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Access token missing");

            // Optional: parse JWT to extract claims
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var sub = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var name = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

            return Ok(new
            {
                sub,
                name
            });
        }
    }
}
