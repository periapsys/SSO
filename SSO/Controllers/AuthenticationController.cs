using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SSO.Business.Authentication;
using SSO.Business.Authentication.Commands;
using SSO.Business.Authentication.Queries;
using SSO.Business.Captchas.Queries;
using SSO.Filters;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SSO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Request for app login
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpGet]
        [AppIdValidator<InitLoginQuery>]
        [ApiExplorerSettings(GroupName = "Client")]
        [EnableCors("AllowAnyOrigin")]
        public async Task<IActionResult> Init([FromQuery] InitLoginQuery form)
        {
            try
            {
                await _mediator.Send(form);

                Response.Cookies.Append("appId", form.ApplicationId!.Value.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = false });

                if (Request.Cookies["token"] != null)
                {
                    var token = await _mediator.Send(new SwitchAppQuery { Token = Request.Cookies["token"], ApplicationId = form.ApplicationId });

                    Response.Cookies.Append("token", token.AccessToken, new CookieOptions { Expires = token.Expires, HttpOnly = false });

                    var callbackUri = new Uri(form.CallbackUrl);
                    var uriBuilder = new UriBuilder(callbackUri);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                    // Add the token parameter
                    query["token"] = token.Id.ToString();
                    uriBuilder.Query = query.ToString();

                    return Redirect(uriBuilder.ToString());
                }

                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={form.ApplicationId}&callbackUrl={form.CallbackUrl}");
            }
            catch (UnauthorizedAccessException)
            {
                // Delete cookie
                Response.Cookies.Delete("token");

                return Redirect($"{Request.Scheme}://{Request.Host}/login?appId={form.ApplicationId}&callbackUrl={form.CallbackUrl}");
            }
        }

        /// <summary>
        /// Authentication for app user
        /// </summary>
        /// <param name="form"></param>
        /// <param name="realmId"></param>
        /// <returns></returns>
        [HttpPost]
        [AppIdValidator<LoginQuery>]
        [ProducesResponseType(typeof(string), 200)]
        [ApiExplorerSettings(GroupName = "System")]
        public async Task<IActionResult> Login([FromBody] LoginQuery form, [FromQuery] Guid? realmId = null)
        {
            var res = await _mediator.Send(form);

            Response.Cookies.Append("token", res.AccessToken, new CookieOptions { Expires = res.Expires, HttpOnly = false });

            return Ok(res);
        }

        /// <summary>
        /// Authentication for system
        /// </summary>
        /// <param name="form"></param>
        /// <param name="realmId"></param>
        /// <returns></returns>
        [HttpPost("system")]
        [ProducesResponseType(typeof(string), 200)]
        [ApiExplorerSettings(GroupName = "System")]
        [RealmIdValidator<LoginToSystemQuery>]
        public async Task<IActionResult> LoginToSystem([FromBody] LoginToSystemQuery form, [FromQuery] Guid? realmId = null)
        {
            form.RealmId = realmId;

            var res = await _mediator.Send(form);

            Response.Cookies.Append("system", res.AccessToken, new CookieOptions { Expires = res.Expires, HttpOnly = false });

            return Ok(res);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        [EnableCors("AllowAnyOrigin")]
        [ApiExplorerSettings(GroupName = "System")]
        public async Task<IActionResult> Logout([FromQuery] Guid? applicationId, [FromQuery, Url(ErrorMessage = "Not a valid Uri.")] string? callbackUrl)
        {
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("system");

            if (applicationId is not null && callbackUrl is not null)
                return await Init(new InitLoginQuery { ApplicationId = applicationId, CallbackUrl = callbackUrl });

            return Redirect($"{Request.Scheme}://{Request.Host}");
        }

        /// <summary>
        /// Gets access-token
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet("token")]
        [EnableCors("AllowAnyOrigin")]
        [ProducesResponseType(typeof(TokenDto), 200)]
        public async Task<IActionResult> GetAccessToken([FromQuery] GetAccessTokenQuery param)
        {
            var res = await _mediator.Send(param);

            return Ok(res);
        }

        /// <summary>
        /// Request for password recovery
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] PasswordRecoveryCommand param, [FromQuery] Guid? realmId = null)
        {
            var baseUrl = !string.IsNullOrEmpty(Request.Headers["Referer"])
                    ? new Uri(Request.Headers["Referer"].ToString()).GetLeftPart(UriPartial.Authority)
                    : $"{Request.Scheme}://{Request.Host}";

            param.BaseUrl = baseUrl;

            await _mediator.Send(new ValidateCaptchaQuery { Captcha = param.Captcha });

            await _mediator.Send(param);

            return Ok();
        }
    }
}
