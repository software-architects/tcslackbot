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
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly SlackConfig _slackConfig;
        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;

        public AuthController(ILogger<AuthController> logger,
            IConfiguration config,
            IHttpClientFactory factory,
            IDataProtectionProvider provider,
            ISecretManager secretManager)
        {
            _logger = logger;
            _configuration = config;
            _factory = factory;
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
        }

        [HttpGet]
        [Route("login")]
        public ActionResult Authenticate([FromQuery] string ReturnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize, HttpGet]
        [Route("link")]
        public async Task<IActionResult> LinkAccounts([FromQuery(Name = "uuid")] string encryptedUuid)
        {
            var refreshToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");

            try
            {
                // Decrypt the uuid
                string decrypedUuid = _protector.Unprotect(encryptedUuid);

                // Associate the uuid with the refresh token
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                await keyVaultClient.SetSecretAsync(Program.GetKeyVaultEndpoint(), decrypedUuid, refreshToken);

                // Reload the configuration because we added new secrets
                ((IConfigurationRoot)_configuration).Reload();
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }

        [Route("test")]
        public async Task<IActionResult> TestingAsync()
        {
            //var refreshToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");
            //Console.WriteLine(refreshToken);

            var accessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            //Console.WriteLine(accessToken);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = await client.GetStringAsync("https://api.timecockpit.com/odata/$metadata");

            return Ok(content);
        }
    }
}
