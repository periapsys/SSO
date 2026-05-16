using MediatR;
using Microsoft.AspNetCore.Mvc;
using SSO.Business.Captchas;
using SSO.Business.Captchas.Queries;

namespace SSO.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Client")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        readonly IMediator _mediator;

        public CaptchaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Generates Captcha
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var param = new GetCaptchaQuery();
            var result = await _mediator.Send(param);
            return Ok(result);
        }

        /// <summary>
        /// Validates Captcha
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] CaptchaRequest request)
        {
            var param = new ValidateCaptchaQuery
            {
                Captcha = request
            };

            await _mediator.Send(param);

            return Ok();
        }
    }
}
