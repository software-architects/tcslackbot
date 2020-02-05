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
using TCSlackbot.Logic.Resources;
using TCSlackbot.Logic.Slack;
using TCSlackbot.Logic.Slack.Requests;
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
        private readonly ITokenManager _tokenManager;
        private readonly ITCManager _tcDataManager;

        private readonly CommandHandler _commandHandler;
        public CommandController(
            IHttpClientFactory factory,
            IDataProtectionProvider provider,
            ISecretManager secretManager,
            ICosmosManager cosmosManager,
            ITokenManager tokenManager,
            ITCManager dataManager
            )
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            if (provider is null)
            {
                var message = "IDataProtectionProvider";
                throw new ArgumentNullException(message);
            }

            if (factory is null)
            {
                throw new ArgumentNullException("IHttpClientFactory");
            }

            if (secretManager is null)
            {
                throw new ArgumentNullException("ISecretManager");
            }
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient("BotClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken"));
            _tokenManager = tokenManager;
            _tcDataManager = dataManager;

            _commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager, _tokenManager, _tcDataManager);
        }

        /// <summary>
        /// Handles the incoming requests (only if they have a valid slack signature).
        /// </summary>
        /// <param name="body">The dynamic request body</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync([FromBody] dynamic body)
        {
            var request = Serializer.Deserialize<SlackBaseRequest>(body.ToString());

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
                    return HandleSlackChallenge(Serializer.Deserialize<SlackChallenge>(body.ToString()));

                case "event_callback":
                    return await HandleEventCallbackAsync(Serializer.Deserialize<SlackEventCallbackRequest>(body.ToString()));

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
            if (request is null)
            {
                return BadRequest();
            }

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
            if (request is null)
            {
                return BadRequest();
            }

            return Ok(request.Challenge);
        }

        /// <summary>
        /// Handles all slack messages and calls the specified command handler if it is a command.
        /// </summary>
        /// <param name="slackEvent"></param>
        /// <returns></returns>
        public async Task<IActionResult> HandleSlackMessage(SlackEvent slackEvent)
        {
            if (slackEvent is null || _commandHandler == null)
            {
                return BadRequest();
            }

            var reply = new Dictionary<string, string>();
            var hiddenMessage = false;
            //
            // Set the reply data
            //

            //reply["token"] = _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken");
            reply["channel"] = slackEvent.Channel;
            reply["user"] = slackEvent.User;

            //
            // Handle the command
            //
            //var user = await _cosmosManager.GetDocumentAsync<SlackUser>(Collection.Users, slackEvent.User);

            var text = slackEvent.Text.Replace("<@UJZLBL7BL> ", "", StringComparison.CurrentCulture).ToLower().Trim().Split(' ').FirstOrDefault();
            switch (text)
            {
                case "login":
                case "link":
                    reply["text"] = _commandHandler.GetLoginLink(slackEvent);
                    hiddenMessage = true;
                    break;

                case "logout":
                case "unlink":
                    //reply["text"] = BotResponses.LogoutMessage;
                    //hiddenMessage = true;
                    //await _secretManager.DeleteSecretAsync(slackEvent.User);
                    //await _cosmosManager.RemoveDocumentAsync(Collection.Users, slackEvent.User);
                    //break;
                    reply["text"] = BotResponses.NotLoggedIn;
                    break;

                // TODO: Reminder after 4h to take a break    
                case "start":
                    reply["text"] = await _commandHandler.StartWorkingAsync(slackEvent);
                    break;

                case "stop":
                    reply["text"] = await _commandHandler.StopWorkingAsync(slackEvent);
                    break;

                // stop@13:00 Maybe add 10 minute break for every 4h
                case "pause":
                case "break":
                    reply["text"] = await _commandHandler.PauseWorktimeAsync(slackEvent);
                    break;

                case "resume":
                    reply["text"] = await _commandHandler.ResumeWorktimeAsync(slackEvent);
                    break;

                case "starttime":
                case "gettime":
                    reply["text"] = await _commandHandler.GetWorktimeAsync(slackEvent);
                    break;

                case "filter":
                    reply["text"] = await _commandHandler.FilterObjectsAsync(slackEvent);
                    break;

                default:
                    break;
            }

            await SendReplyAsync(reply, hiddenMessage);

            return Ok();
        }

        private async void ReminderScheduler(int hour, int min, double intervalInHour, Action task)
        {
            _ = hour;
            _ = min;
            _ = intervalInHour;
            _ = task;

            var queryable = _cosmosManager.GetAllSlackUsers();
            if (queryable == null)
            {
                return;
            }

            while (queryable.HasMoreResults)
            {
                // Iterate through SlackUsers
                foreach (SlackUser s in await queryable.ExecuteNextAsync<SlackUser>())
                {
                    var reply = new Dictionary<string, string>
                    {
                        ["user"] = s.UserId,
                        ["channel"] = s.ChannelId,
                        ["text"] = BotResponses.TakeABreak
                    };

                    await SendReplyAsync(reply, true);
                }
            }
        }

        /// <summary>
        /// Get the IM channel id from user
        /// </summary>
        /// <param name="user">Id of user</param>
        /// <returns>channel id</returns>
        public static async Task<string?> GetIMChannelFromUserAsync(HttpClient client, string user)
        {
            if (client is null)
            {
                return default;
            }

            var list = await client.GetAsync(new Uri("https://slack.com/api/conversations.list?types=im"));
            foreach (var channel in Serializer.Deserialize<ConversationsList>(await list.Content.ReadAsStringAsync()).Channels)
            {
                if (channel.User == user)
                {
                    return channel.Id;
                }
            }

            return default;
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
                replyData["channel"] = GetIMChannelFromUserAsync(_httpClient, replyData["user"]).Result;
                requestUri = "chat.postEphemeral";
            }

            using var content = new FormUrlEncodedContent(replyData);
            _ = await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, requestUri), content);
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
            var ownSignature = $"v0={BitConverter.ToString(hash).Replace("-", "", StringComparison.CurrentCulture).ToLower()}";

            return ownSignature.Equals(signature, StringComparison.CurrentCulture);
        }
    }
}
