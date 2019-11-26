﻿using Microsoft.AspNetCore.Authentication;
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
    }
}
