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
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Authentication;
using TCSlackbot.Logic.Distribution;
using TCSlackbot.Logic.Utils;
namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("distribution")]
    public class DistributionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public DistributionController(IConfiguration configuration, IHttpClientFactory factory)
        {
            _configuration = configuration;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Handles the distribution request.
        /// </summary>
        /// <param name="state">The state parameter set in the distribution link.</param>
        /// <param name="code">The temporary code that can be used to send a request to the slack oauth endpoint.</param>
        /// <returns>Ok if valid. If the oauth response was not valid, the error message will be returned.</returns>
        [HttpGet]
        public async Task<IActionResult> DistributeAsync([FromQuery] string? state, [FromQuery] string code)
        {
            if (_client is null)
            {
                return BadRequest();
            }

            // See these links as reference for the distribution:
            // - https://api.slack.com/methods/oauth.access
            // - https://api.slack.com/methods/oauth.v2.access
            // - https://api.slack.com/authentication/oauth-v2
            // - https://github.com/slackapi/bolt/issues/390#issuecomment-583207021
            //
            // The login link can be generated here: https://api.slack.com/docs/slack-button
            //

            var form = new Dictionary<string, string>
            {
                { "client_id", _configuration["Slack-ClientId"] },
                { "client_secret", _configuration["Slack-ClientSecret"] },
                { "code", code },
            };

            using var data = new FormUrlEncodedContent(form);
            var result = await _client.PostAsync("https://slack.com/api/oauth.access", data);
            var stringContent = await result.Content.ReadAsStringAsync();
            var content = Serializer.Deserialize<DistributionResponse>(stringContent);

            // Show error if invalid
            //
            if (content == null || content.Bot == null || !content.Ok)
            {
                return BadRequest(content?.Error);
            }

            // Add the TeamId and BotAccessToken to the key vault to be able to access the workspace
            //
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            await keyVaultClient.SetSecretAsync(Program.GetKeyVaultEndpoint(), content.TeamId, content.Bot.BotAccessToken);

            // Reload the configuration because we added a new secret
            ((IConfigurationRoot)_configuration).Reload();

            return Ok("Successfully added the application");
        }
    }
}
