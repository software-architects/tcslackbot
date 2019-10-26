using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TCSlackbot.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("auth")]
        public IActionResult Authenticate()
        {
            return Ok("Hello World");
        }
    }
}
