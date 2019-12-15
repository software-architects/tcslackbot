using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Slack;
using TCSlackbot.Logic.Utils;

namespace TCSlackbot.Controllers
{

    [ApiController]
    [Route("command")]
    public class CommandController : ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;
        private readonly ICosmosManager _cosmosManager;
        private readonly HttpClient _httpClient;

        private readonly CommandHandler commandHandler;

        public CommandController(IDataProtectionProvider provider, ISecretManager secretManager, ICosmosManager cosmosManager, IHttpClientFactory factory)
        {
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient("BotClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken"));
            commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager);
        }

        /// <summary>
        /// Handles the incoming requests (only if they have a valid slack signature).
        /// </summary>
        /// <param name="body">The dynamic request body</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync([FromBody] dynamic body)
        {
            var request = Deserialize<SlackBaseRequest>(body.ToString());

            //
            // Verify slack request
            //
            if (!IsValidSignature(body.ToString(), HttpContext.Request.Headers))
            {
                return BadRequest();
            }

            //
            // Handle the request
            //
            switch (request.Type)
            {
                case "url_verification":
                    return HandleSlackChallenge(Deserialize<SlackChallenge>(body.ToString()));

                case "event_callback":
                    return await HandleEventCallbackAsync(Deserialize<SlackEventCallbackRequest>(body.ToString()));

                default:
                    Console.WriteLine($"Received unhandled request: {request.Type}.");
                    break;
            }

            return NotFound();
        }

        /// <summary>
        /// Handles the requests with the type 'event_callback' and calls the specified event handler.
        /// </summary>
        /// <param name="request">The event request data</param>
        /// <returns></returns>
        public async Task<IActionResult> HandleEventCallbackAsync(SlackEventCallbackRequest request)
        {
            switch (request.Event.Type)
            {
                case "message":
                    return await HandleSlackMessage(request.Event);

                case "app_mention":
                    return await HandleSlackMessage(request.Event);

                default:
                    break;
            }

            return NotFound();
        }

        /// <summary>
        /// Handles the slack challenge (Needed for setting up event subscriptions). 
        /// </summary>
        /// <param name="request">The slack challenge request</param>
        /// <returns>The challenge property of the challenge request</returns>
        public IActionResult HandleSlackChallenge(SlackChallenge request)
        {
            return Ok(request.Challenge);
        }

        /// <summary>
        /// Handles all slack messages and calls the specified command handler if it is a command.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns></returns>
        public async Task<IActionResult> HandleSlackMessage(SlackEvent slackEvent)
        {
            var reply = new Dictionary<string, string>();
            var directMessage = false;

            //
            // Set the reply data
            //
            
            //reply["token"] = _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken");
            reply["channel"] = slackEvent.Channel;
            reply["user"] = slackEvent.User;

            //
            // Handle the command
            //
            var text = slackEvent.Text.Replace("<@UJZLBL7BL> ", "").ToLower().Trim().Split("");
            switch (slackEvent.Text.ToLower().Trim().Split(" ").FirstOrDefault())
            {
                case "login":
                case "link":
                    reply["text"] = commandHandler.GetLoginLink(slackEvent);
                    directMessage = true;
                    break;

                // TODO: Reminder after 4h to take a break    
                case "start":
                    reply["text"] = await commandHandler.StartWorkingAsync(slackEvent);
                    break;

                case "stop":
                    reply["text"] = await commandHandler.StopWorkingAsync(slackEvent);
                    break;

                // stop@13:00 Maybe add 10 minute break for every 4h
                case "pause":
                case "break":
                    reply["text"] = await commandHandler.PauseWorktimeAsync(slackEvent);
                    break;

                case "resume":
                    reply["text"] = await commandHandler.ResumeWorktimeAsync(slackEvent);
                    break;

                case "starttime":
                case "gettime":
                    reply["text"] = await commandHandler.GetWorktimeAsync(slackEvent);
                    break;

                case "filter":
                    reply["text"] = await commandHandler.FilterObjectsAsync(slackEvent);
                    break;

                default:
                    break;
            }

            await SendReplyAsync(reply, directMessage);

            return Ok();
        }

        /// <summary>
        /// Sends a reply either in the group channel or via direct message.
        /// </summary>
        /// <param name="replyData">The data of the reply (message, channel, ...)</param>
        /// <param name="directMessage">True when it should be sent via direct message</param>
        /// <returns></returns>
        private async Task SendReplyAsync(Dictionary<string, string> replyData, bool directMessage)
        {
            string requestUri = "chat.postMessage";

            //
            // Use a different uri for the direct message
            //
            if (directMessage)
            {
                requestUri = "chat.postEphemeral";
            }

            await _httpClient.PostAsync(requestUri, new FormUrlEncodedContent(replyData));
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
