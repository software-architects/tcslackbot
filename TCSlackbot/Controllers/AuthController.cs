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
using System.Threading.Tasks;
using TCSlackbot.Logic;

namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        public static readonly AccessTokenCache accessTokenCache = new AccessTokenCache();

        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _protector;

        public AuthController(ILogger<AuthController> logger,
            IConfiguration config,
            IDataProtectionProvider provider)
        {
            _logger = logger;
            _configuration = config;
            _protector = provider.CreateProtector("UUIDProtector");
        }

        /// <summary>
        /// Authenticates the users with open id.
        /// </summary>
        /// <param name="ReturnUrl">The location to return to when authenticated</param>
        /// <returns></returns>
        [HttpGet]
        [Route("login")]
        public ActionResult Authenticate([FromQuery] string ReturnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Allows the user to link the slack with the TimeCockpit account.
        /// </summary>
        /// <param name="encryptedUuid">The encrypted id of the slack user</param>
        /// <returns></returns>
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

                // Reload the configuration because we added a new secret
                ((IConfigurationRoot)_configuration).Reload();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception.ToString());
                return BadRequest("Failed to login.");
            }

            return Ok("Successfully logged in.");
        }

        [Route("test")]
        public async Task<IActionResult> Test()
        {
            // WORKING
            //var secretManager = new SecretManager(_configuration);
            //var manager = new TokenManager(_configuration, secretManager);

            //var result = await manager.RenewTokensAsync("<refresh_token>");
            //Console.WriteLine(result);

            // TODO: 
            // - https://identitymodel.readthedocs.io/en/latest/client/token.html
            // - https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-3.1

            return Ok();
        }
    }
}
