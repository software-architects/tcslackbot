using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }

        [HttpGet]
        public IActionResult Authenticate()
        {
            return Ok();
        }
    }
}
