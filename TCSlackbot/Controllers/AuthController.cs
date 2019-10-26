using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
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
        private readonly IBotClient _botClient;

        private readonly SlackConfig _slackConfig;

        public AuthController(ILogger<AuthController> logger, IConfiguration config, ISecretManager secretManager, IBotClient botClient, IOptions<SlackConfig> slackConfig)
        {
            _logger = logger;
            _configuration = config;
            _secretManager = secretManager;
            _botClient = botClient;
            _slackConfig = slackConfig.Value ?? throw new ArgumentException(nameof(SlackConfig));
        }

        [HttpGet]
        public async Task<IActionResult> Authenticate()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            Console.WriteLine(accessToken);

            return Ok(_secretManager.GetSecret("mySecret"));
        }

        [HttpGet]
        [Route("startBot")]
        public IActionResult Login()
        {
            _botClient.Test(_slackConfig);
            return Ok("Started Bot");
        }
    }
}
