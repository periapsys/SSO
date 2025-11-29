using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SSO.Business.Authentication.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SSO.Controllers
{
    [ApiController]
    public class OidcController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly RsaSecurityKey _rsaKey;

        public OidcController(IMediator mediator)
        {
            _mediator = mediator;

            // Load RSA private key from PEM file
            var pemFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private_key.pem");
            var privateKeyText = System.IO.File.ReadAllText(pemFilePath);

            RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyText.ToCharArray());

            _rsaKey = new RsaSecurityKey(rsa)
            {
                KeyId = Guid.NewGuid().ToString() // Generate a unique KeyId
            };
        }

        [HttpGet]
        [Route(".well-known/openid-configuration")]
        public IActionResult Discovery()
        {
            var issuer = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                issuer,
                authorization_endpoint = $"{issuer}/connect/authorize",
                token_endpoint = $"{issuer}/connect/token",
                userinfo_endpoint = $"{issuer}/connect/userinfo",
                jwks_uri = $"{issuer}/.well-known/jwks.json",
                response_types_supported = new[] { "code" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { "RS256" }
            });
        }

        [HttpGet]
        [Route(".well-known/jwks.json")]
        public IActionResult Jwks()
        {
            var parameters = _rsaKey.Rsa.ExportParameters(false);
            var key = new
            {
                kty = "RSA",
                use = "sig",
                kid = _rsaKey.KeyId,
                e = Base64UrlEncoder.Encode(parameters.Exponent),
                n = Base64UrlEncoder.Encode(parameters.Modulus)
            };

            return Ok(new { keys = new[] { key } });
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

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = res.AccessToken;

            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var givenName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? "";
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value ?? "";

            // ID token claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("name", givenName),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, "SSO"), // Must match client_id in EasyAuth
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Sign ID token using RS256
            var idToken = new JwtSecurityToken(
                issuer: issuer,
                audience: "SSO",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: res.Expires,
                signingCredentials: new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256)
            );

            return new JsonResult(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = (int)(res.Expires - DateTime.UtcNow).TotalSeconds,
                id_token = tokenHandler.WriteToken(idToken)
            });
        }

        [HttpGet("connect/userinfo")]
        public IActionResult UserInfo()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Access token missing");

            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var sub = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var name = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

            return Ok(new { sub, name });
        }
    }
}
