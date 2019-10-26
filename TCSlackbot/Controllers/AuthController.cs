using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TCSlackbot.Logic;

namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISecretManager _secretManager;

        public AuthController(ILogger<AuthController> logger, IConfiguration config, ISecretManager secretManager)
        {
            _logger = logger;
            _configuration = config;
            _secretManager = secretManager;
        }

        [HttpGet]
        public IActionResult Authenticate()
        {
            return Ok(_secretManager.GetSecret("mySecret"));
        }
    }
}
