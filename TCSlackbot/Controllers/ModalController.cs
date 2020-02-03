using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Cosmos;
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
        private readonly ITokenManager _tokenManager;
        private readonly ITCDataManager _tCDataManager;
        private readonly CommandHandler commandHandler;

        public ModalController(IDataProtectionProvider provider, ISecretManager secretManager, ICosmosManager cosmosManager, IHttpClientFactory factory, ITokenManager tokenManager, ITCDataManager dataManager)
        {
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient("BotClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken"));
            _tokenManager = tokenManager;
            _tCDataManager = dataManager;
            commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager, _tokenManager, _tCDataManager);
        }

        /// <summary>
        /// Handles the incoming requests (only if they have a valid slack signature).
        /// </summary>
        /// <param name="body">The dynamic request body</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync()
        {
            //
            // Verify slack request
            //
            // TODO: Make it work in Modals
            //
            // if (!IsValidSignature(HttpContext.Request.Body.ToString(), HttpContext.Request.Headers))
            // {
            //    return BadRequest();
            // }

            var payload = Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            //
            // Ignore unecessary requests
            //
            if (payload.Type == "block_actions" || payload.Type == "view_closed")
                return Ok();
           
            //
            // Check if user is logged in, working
            //
            var user = await commandHandler.GetSlackUserAsync(payload.User.Id);
            if (user == null)
            {
                // replyData["text"] = BotResponses.NotLoggedIn;
                // await _httpClient.PostAsync("chat.postEphemeral", new FormUrlEncodedContent(replyData));
                return Ok();
            }
            if (!user.IsWorking)
            {
                // replyData["text"] = BotResponses.NotWorking;
                // await _httpClient.PostAsync("chat.postEphemeral", new FormUrlEncodedContent(replyData));                
                return Ok();
            }

            switch (payload.Type)
            {
                case "message_action":
                    return await ViewModalAsync(payload);
                case "view_submission":
                    return await ProcessModalDataAsync(user);    /* , replyData */
                default:
                    Console.WriteLine($"Received unhandled request: {payload.Type}.");
                    break;
            }
            return Ok();
        }


        public async Task<IActionResult> ViewModalAsync(AppActionPayload payload)
        {
            string json = "{\"trigger_id\": \"" + payload.TriggerId + "\", \"view\": { \"type\": \"modal\", \"callback_id\": \"" + payload.CallbackId + "\",";
            json += await System.IO.File.ReadAllTextAsync("Json/StopTimeTracking.json");
            await _httpClient.PostAsync("views.open", new StringContent(json, Encoding.UTF8, "application/json"));
            return Ok(json);
        }
        public async Task<IActionResult> ProcessModalDataAsync(SlackUser user)   /* , Dictionary<string,string> replyData */
        {
            var payload = JsonSerializer.Deserialize<SlackViewSubmission>(HttpContext.Request.Form["payload"]);
            
            TimeSpan startTime;
            TimeSpan endTime;
            String errorMessage = "{ \"response_action\": \"errors\", \"errors\": {";
            if (payload.View.State.Values.Date.Date.Day  == null)
            {
                // TODO: send message to user
                return Ok();
            }
            if (!TimeSpan.TryParse(payload.View.State.Values.Starttime.StartTime.Value, out startTime)) 
            {
                // TODO: send message to user
                errorMessage += "\"starttime\": \"Please use a valid time format! (eg. \"08:00\")\",";
            }
            if (!TimeSpan.TryParse(payload.View.State.Values.Endtime.EndTime.Value, out endTime))
            {
                // TODO: send message to user
                errorMessage += "\"endtime\": \"Please use a valid time format! (eg. \"08:00\")\",";
            }
            if (endTime.CompareTo(startTime) != 1)
            {
                errorMessage += "\"endtime\": \"End Time has to be after Start Time!";
            }
            if (errorMessage.EndsWith(","))
            {
                errorMessage = errorMessage.Substring(0, errorMessage.Length - 1) + "}}";
                /*
                var replyData = new Dictionary<string, string>();
                replyData["user"] = payload.User.Id;
                replyData["text"] = errorMessage;

                await _httpClient.PostAsync("https://747773f7.ngrok.io/modal", new FormUrlEncodedContent(replyData));
                */
                return Ok();
            }

            DateTime date = payload.View.State.Values.Date.Date.Day;
            
            user.StartTime = date + startTime; // startTime 

            user.EndTime = date + endTime;
            
            user.Description = payload.View.State.Values.Description.Description.Value;
            user.IsWorking = false;
            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);
            var replyData = new Dictionary<string, string>();
            replyData["user"] = payload.User.Id;
            replyData["channel"] = await GetIMChannelFromUserAsync(replyData["user"]);
            replyData["text"] = "Your time has been saved saved";
            await _httpClient.PostAsync("chat.postEphemeral", new FormUrlEncodedContent(replyData));
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
        /// Get the IM channel id from user
        /// </summary>
        /// <param name="user">Id of user</param>
        /// <returns>channel id</returns>
        public async Task<string> GetIMChannelFromUserAsync(string user)
        {
            var list = await _httpClient.GetAsync("https://slack.com/api/conversations.list?types=im");
            foreach (var channel in JsonSerializer.Deserialize<ConversationsList>(await list.Content.ReadAsStringAsync()).Channels)
            {
                if (channel.User == user)
                {
                    return channel.Id;
                }
            }
            return null;
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