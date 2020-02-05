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
            if (provider is null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException("IDataProtectionProvider");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

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
        public IActionResult Authenticate([FromQuery] Uri ReturnUri)
        {
            if (ReturnUri == null)
            {
                ReturnUri = new Uri("/");
            }

            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUri.AbsoluteUri }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Allows the user to link the slack with the TimeCockpit account.
        /// </summary>
        /// <param name="encryptedData">The encrypted id of the slack user</param>
        /// <returns></returns>
        [Authorize, HttpGet]
        [Route("link")]
        public async Task<IActionResult> LinkAccounts([FromQuery(Name = "data")] string encryptedData)
        {
            var refreshToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");

            try
            {
                // Decrypt the data
                string decryptedData = _protector.Unprotect(encryptedData);

                // Deserialize json
                var jsonData = Serializer.Deserialize<LinkData>(decryptedData);

                // Validate the time
                if (DateTime.Now > jsonData.ValidUntil)
                {
                    return BadRequest("Link has expired.");
                }

                // Associate the uuid with the refresh token
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                await keyVaultClient.SetSecretAsync(Program.GetKeyVaultEndpoint(), jsonData.UserId, refreshToken);

                // Reload the configuration because we added a new secret
                ((IConfigurationRoot)_configuration).Reload();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception.ToString());
                return BadRequest("Failed to link accounts.");
            }

            return Ok("Successfully linked the accounts.");
        }
    }
}
