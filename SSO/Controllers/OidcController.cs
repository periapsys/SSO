using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SSO.Business.Authentication.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SSO.Controllers
{
    [EnableCors("AllowAnyOrigin")]
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

            // Stable KeyId based on public key hash
            var pubKey = rsa.ExportRSAPublicKey();
            var keyId = Base64UrlEncoder.Encode(SHA256.HashData(pubKey));

            _rsaKey = new RsaSecurityKey(rsa) { KeyId = keyId };
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
                end_session_endpoint = $"{issuer}/connect/endsession",
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
            var nonce = Request.Query["nonce"].ToString(); // may be empty

            await _mediator.Send(new InitLoginQuery { ApplicationId = clientId, CallbackUrl = redirectUri });

            // Save appId and nonce in cookies for token endpoint
            Response.Cookies.Append("appId", clientId.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = false });
            Response.Cookies.Append("oidc_nonce", nonce, new CookieOptions { Expires = DateTime.Now.AddMinutes(10), HttpOnly = true });

            if (Request.Cookies["token"] != null)
            {
                var token = await _mediator.Send(new SwitchAppQuery { Token = Request.Cookies["token"], ApplicationId = clientId });
                var token1 = await _mediator.Send(new GetAccessTokenQuery { RequestToken = token.Id });
                Response.Cookies.Append("token", token.AccessToken, new CookieOptions { Expires = token.Expires, HttpOnly = false });

                var redirect = $"{redirectUri}?code={token.Id}&state={state}";
                return Redirect(redirect);
            }

            return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={clientId}&callbackUrl={redirectUri}&state={state}");
        }

        [HttpPost("connect/token")]
        public async Task<IActionResult> Token()
        {
            var code = Request.Form["code"];
            var clientId = Request.Form["client_id"].ToString();
            var issuer = $"{Request.Scheme}://{Request.Host}";

            var res = await _mediator.Send(new GetAccessTokenQuery { RequestToken = Guid.Parse(code!) });
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = res.AccessToken;

            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var givenName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? "";
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value ?? "";

            // Read nonce from cookie (may be empty)
            var nonce = Request.Cookies["oidc_nonce"] ?? "";
            Response.Cookies.Delete("oidc_nonce"); // single-use

            // ID token claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("name", givenName),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, clientId),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Nonce, nonce) // ✅ required by EasyAuth
            };

            var idToken = new JwtSecurityToken(
                issuer: issuer,
                audience: clientId,
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

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _rsaKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                }, out _);

                var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var sub = token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                var name = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

                return Ok(new { sub, name });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpGet("connect/endsession")]
        public IActionResult EndSession([FromQuery] string? post_logout_redirect_uri, [FromQuery] string? id_token_hint)
        {
            // Clear the authentication cookies (your token cookie)
            Response.Cookies.Delete("token");

            // Optionally clear other OIDC state cookies
            Response.Cookies.Delete("oidc_nonce");
            Response.Cookies.Delete("appId");

            if (!string.IsNullOrEmpty(post_logout_redirect_uri))
                return Redirect(post_logout_redirect_uri);

            return Ok("Logged out");
        }
    }
}
