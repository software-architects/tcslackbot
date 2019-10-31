using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.Webhooks;
using System;
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
        private readonly SlackConfig _slackConfig;

        public AuthController(ILogger<AuthController> logger, IConfiguration config, ISecretManager secretManager, IOptions<SlackConfig> slackConfig)
        {
            _logger = logger;
            _configuration = config;
            _secretManager = secretManager;
            _slackConfig = slackConfig.Value ?? throw new ArgumentException(nameof(SlackConfig));
        }

        [HttpGet]
        public IActionResult Authenticate()
        {
            return Ok(_secretManager.GetSecret("mySecret"));
        }
        
    }
}
