using MediatR;
using Microsoft.AspNetCore.Mvc;
using SSO.Business.Authentication.Queries;
using System.Security.Cryptography;
using System.Web;

namespace SSO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OidcController : ControllerBase
    {

        private readonly IMediator _mediator;

        public OidcController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Returns the OpenID Connect discovery document containing configuration information for clients to interact
        /// with the authorization server.
        /// </summary>
        /// <remarks>The returned discovery document follows the OpenID Connect standard and includes
        /// endpoint URLs based on the current request's scheme and host. Clients use this document to dynamically
        /// configure themselves for authentication and authorization flows.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object that describes the OpenID Connect endpoints,
        /// supported scopes, response types, and other metadata required for client integration.</returns>
        [HttpGet("~/.well-known/openid-configuration")]
        public IActionResult GetOpenIdConfiguration()
        {
            var issuer = $"{Request.Scheme}://{Request.Host}";
            var configuration = new
            {
                issuer = issuer,
                authorization_endpoint = $"{issuer}/api/oidc/authorize",
                token_endpoint = $"{issuer}/api/oidc/token",
                userinfo_endpoint = $"{issuer}/api/oidc/userinfo",
                jwks_uri = $"{issuer}/.well-known/jwks.json",
                response_types_supported = new[] { "code", "token", "id_token", "code token", "code id_token", "token id_token", "code token id_token" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { "RS256" },
                scopes_supported = new[] { "openid", "profile", "email", "offline_access" },
                token_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post" },
                claims_supported = new[] { "sub", "name", "preferred_username", "email", "realm", "role" }
            };
            return Ok(configuration);
        }

        /// <summary>
        /// Initiates the OAuth 2.0 authorization flow by validating the client and redirecting the user to the
        /// specified URI with an authorization code or error response.
        /// </summary>
        /// <remarks>This endpoint is typically used as part of the OAuth 2.0 authorization code grant
        /// flow. The caller should ensure that all parameters meet the requirements of the authorization server and
        /// that the redirect URI is properly registered.</remarks>
        /// <param name="client_id">The unique identifier of the client application requesting authorization. Must be registered with the
        /// authorization server.</param>
        /// <param name="redirect_uri">The URI to which the authorization response will be sent. Must match a registered redirect URI for the
        /// client.</param>
        /// <param name="response_type">The type of response desired from the authorization endpoint. Typically set to "code" for authorization code
        /// flow.</param>
        /// <param name="state">An optional value used to maintain state between the request and callback. If provided, it will be included
        /// in the response.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects the user to the specified <paramref name="redirect_uri"/> with
        /// the authorization response parameters.</returns>
        /// <exception cref="NotImplementedException">Thrown in all cases as the method is not yet implemented.</exception>
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize([FromQuery] string client_id, [FromQuery] string redirect_uri,
        [FromQuery] string response_type = "code", [FromQuery] string? state = null)
        {
            try
            {
                // Spec parameters (who is calling + where they want to go)
                if (string.IsNullOrWhiteSpace(client_id) ||
                    string.IsNullOrWhiteSpace(redirect_uri) ||
                    response_type != "code")
                {
                    return BadRequest(new { error = "invalid_request" });
                }

                await _mediator.Send(new InitLoginQuery { ApplicationId = new Guid(client_id), CallbackUrl = redirect_uri });

                Response.Cookies.Append("appId", client_id, new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = false });

                if (Request.Cookies["token"] != null)
                {
                    var token = await _mediator.Send(new SwitchAppQuery { Token = Request.Cookies["token"], ApplicationId = new Guid(client_id) });

                    Response.Cookies.Append("token", token.AccessToken, new CookieOptions { Expires = token.Expires, HttpOnly = false });

                    var callbackUri = new Uri(redirect_uri);
                    var uriBuilder = new UriBuilder(callbackUri);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                    query["code"] = token.Id.ToString();

                    if (!string.IsNullOrEmpty(state))
                        query["state"] = state;

                    uriBuilder.Query = query.ToString();

                    return Redirect(uriBuilder.ToString());
                }

                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={client_id}&callbackUrl={redirect_uri}");
            }
            catch (UnauthorizedAccessException)
            {
                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={client_id}&callbackUrl={redirect_uri}");
            }
        }

        /// <summary>
        /// Handles an OAuth 2.0 token request and returns an access token response based on the provided authorization
        /// code and client credentials.
        /// </summary>
        /// <remarks>This endpoint implements the OAuth 2.0 authorization code grant flow. Ensure that all
        /// parameters are provided and valid to successfully obtain an access token. The response format follows OAuth
        /// 2.0 specifications.</remarks>
        /// <param name="grant_type">The OAuth 2.0 grant type for the token request. Must be set to "authorization_code" for this endpoint.</param>
        /// <param name="code">The authorization code received from the authorization endpoint. Must be a valid, non-empty string
        /// representing a GUID.</param>
        /// <param name="redirect_uri">The redirect URI associated with the authorization code. Must match the URI used during the authorization
        /// request.</param>
        /// <param name="client_id">The client identifier registered with the authorization server. Used to authenticate the client making the
        /// request.</param>
        /// <param name="client_secret">The client secret associated with the client identifier. Used to verify the client's identity.</param>
        /// <returns>An <see cref="IActionResult"/> containing the access token response if the request is valid; otherwise, an
        /// error response indicating the reason for failure.</returns>
        [HttpPost("token")]
        public async Task<IActionResult> Token([FromForm] string grant_type,
                                       [FromForm] string code,
                                       [FromForm] string redirect_uri,
                                       [FromForm] string client_id,
                                       [FromForm] string client_secret)
        {
            var res = await _mediator.Send(new GetAccessTokenQuery
            {
                RequestToken = new Guid(code)
            });

            return Ok(res);
        }

        /// <summary>
        /// Retrieves the JSON Web Key Set (JWKS) containing the public RSA key used for token signature validation.
        /// </summary>
        /// <remarks>The JWKS is served at the standard OpenID Connect discovery endpoint and can be
        /// consumed by clients or identity providers to validate tokens issued by this application. The key set
        /// includes the modulus and exponent of the RSA public key, encoded using base64url as specified by the JWKS
        /// standard.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the JWKS in JSON format. The response includes the public RSA key
        /// parameters required for verifying JWT signatures.</returns>
        [HttpGet("~/.well-known/jwks.json")]
        public IActionResult GetJwks()
        {
            var pem = System.IO.File.ReadAllText("public_key.pem");
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            var p = rsa.ExportParameters(false);

            var n = Convert.ToBase64String(p.Modulus).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            var e = Convert.ToBase64String(p.Exponent).Replace("+", "-").Replace("/", "_").TrimEnd('=');

            return Ok(new
            {
                keys = new[] {
                new {
                    kty = "RSA",
                    use = "sig",
                    alg = "RS256",
                    kid = "my-key-id",
                    n,
                    e
                    }
                }
            });
        }
    }
}
