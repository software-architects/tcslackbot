using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Slack;
using TCSlackbot.Logic.Slack.Requests;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot.Controllers
{
    [ApiController]
    [Route("modal")]
    public class ModalController : ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;
        private readonly ICosmosManager _cosmosManager;
        private readonly HttpClient _httpClient;

        private readonly CommandHandler commandHandler;

        public ModalController(IDataProtectionProvider provider, ISecretManager secretManager, ICosmosManager cosmosManager, IHttpClientFactory factory)
        {
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient("BotClient");

            commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager);
        }

        /// <summary>
        /// Handles the incoming requests (only if they have a valid slack signature).
        /// </summary>
        /// <param name="body">The dynamic request body</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync()
        {
            string key = HttpContext.Request.Form.Keys.First().ToString();
            StringValues output;
            HttpContext.Request.Form.TryGetValue(key, out output);
            
            var payload = Deserialize<AppActionPayload>(output.ToString());

            //
            // Verify slack request
            //
            //if (!IsValidSignature(body.ToString(), HttpContext.Request.Headers))
            //{
            //    return BadRequest();
            //}

            //
            // Handle the request
            //
            switch (payload.Type)
            {
                case "message_action": 
                    Console.WriteLine("User used an app action"); break;
                //case "block_action ": 
                //    Console.WriteLine("This is a block action"); break;

                default:
                    Console.WriteLine($"Received unhandled request: {payload.Type}.");
                    break;
            }

            return NotFound();
        }

        public async Task<IActionResult> HandleSlackMessage(SlackEvent slackEvent)
        {
            var reply = new Dictionary<string, string>();
            //
            // Set the reply data
            // https://api.slack.com/surfaces/modals/using 3.0 Opening a Modal
           // reply[]
            //
            // Handle the command
            //
            //var json = await System.IO.File.ReadAllTextAsync("../Json/StopTimeTracking.json");
            //var payload = JsonSerializer.Deserialize<AppActionPayload>(json);

//            await _httpClient.PostAsync("views.open", new FormUrlEncodedContent(payload));

            return Ok();
        }

        /// <summary>
        /// Validates the signature of the slack request.
        /// </summary>
        /// <param name="body">The request body</param>
        /// <param name="headers">The request headers</param>
        /// <returns>True if the signature is valid</returns>
        private bool IsValidSignature(string body, IHeaderDictionary headers)
        {
            var timestamp = headers["X-Slack-Request-Timestamp"];
            var signature = headers["X-Slack-Signature"];
            var signingSecret = _secretManager.GetSecret("Slack-SigningSecret");

            var encoding = new UTF8Encoding();
            using var hmac = new HMACSHA256(encoding.GetBytes(signingSecret));
            var hash = hmac.ComputeHash(encoding.GetBytes($"v0:{timestamp}:{body}"));
            var ownSignature = $"v0={BitConverter.ToString(hash).Replace("-", "").ToLower()}";

            return ownSignature.Equals(signature);
        }

        /// <summary>
        /// Deserializes the specified content to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data</typeparam>
        /// <param name="content">The serialized content</param>
        /// <returns>The deserialized object of the specified type</returns>
        private static T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}