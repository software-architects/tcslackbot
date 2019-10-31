using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
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
        private readonly IHttpClientFactory _factory;
        private readonly ISecretManager _secretManager;
        private readonly IBotClient _botClient;

        private readonly SlackConfig _slackConfig;

        public AuthController(ILogger<AuthController> logger, IConfiguration config, IHttpClientFactory factory, ISecretManager secretManager, IBotClient botClient, IOptions<SlackConfig> slackConfig)
        {
            _logger = logger;
            _configuration = config;
            _factory = factory;
            _secretManager = secretManager;
            _botClient = botClient;
            _slackConfig = slackConfig.Value ?? throw new ArgumentException(nameof(SlackConfig));
        }

        [HttpGet]
        [Route("login")]
        public ActionResult Authenticate(string redirectUri = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize, HttpGet]
        [Route("test")]
        public async Task<IActionResult> SomeFunction()
        {
            var httpClient = _factory.CreateClient("APIClient");
            var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            return Ok(token);
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
