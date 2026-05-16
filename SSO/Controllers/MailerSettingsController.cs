using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSO.Business.MailerSettings.Commands;
using SSO.Business.MailerSettings.Queries;

namespace SSO.Controllers
{
    [ApiExplorerSettings(GroupName = "System")]
    [Route("api/realm/mailer")]
    [ApiController]
    [Authorize(Policy = "RealmAccessPolicy")]
    public class MailerSettingsController : ControllerBase
    {
        readonly IMediator _mediator;

        public MailerSettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates or updates Mailer settings
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] ModifyMailerSettingsCommand param)
        {
            param.RealmId = new Guid(User.Claims.First(x => x.Type == "realm").Value);

            await _mediator.Send(param);

            return Ok();
        }

        /// <summary>
        /// Gets Mailer settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = new GetMailerSettingsQuery
            {
                RealmId = new Guid(User.Claims.First(x => x.Type == "realm").Value)
            };

            var res = await _mediator.Send(query);

            return Ok(res);
        }
    }
}
