using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Authentication;
using TCSlackbot.Logic.Utils;
namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("distribution")]
    public class DistributionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DistributionController(IConfiguration configuration) {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Distribute([FromQuery] string? state, [FromQuery] string code)
        {
            return Redirect($"https://slack.com/oauth/authorize?state={state}&client_id=645682850067.645685522130&scope=app_mentions:read,channels:history,channels:read,chat:write,commands,groups:history,groups:read,im:history,im:read,im:write,users:read&user_scope=channels:read,groups:read,identify,im:read,im:write,users.profile:read,users:read");
        }
    }
}
