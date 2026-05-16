using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SSO.Business.Captchas;

namespace SSO.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Client")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        readonly IMemoryCache _cache;

        public CaptchaController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Generates Captcha
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var text = Random.Shared.Next(1000, 9999).ToString();
            var id = Guid.NewGuid().ToString();

            _cache.Set($"captcha:{id}", text, TimeSpan.FromMinutes(5));

            var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="120" height="40">
              <rect width="100%" height="100%" fill="#f3f3f3"/>
              <text x="15" y="28"
                    font-size="24"
                    font-family="monospace"
                    fill="#333">{text}</text>
            </svg>
            """;

            return Ok(new
            {
                id,
                image = $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}"
            });
        }

        /// <summary>
        /// Validates Captcha
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] CaptchaRequest request)
        {
            if (!_cache.TryGetValue($"captcha:{request.Id}", out string? answer))
                return BadRequest("Expired");

            if (answer != request.Answer)
                return BadRequest("Invalid");

            _cache.Remove($"captcha:{request.Id}");

            return Ok();
        }
    }
}
