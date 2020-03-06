using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TCSlackbot.Logic.TimeCockpit.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic;
using TCSlackbot.Logic.Authentication.Exceptions;
using TCSlackbot.Logic.Cosmos;
using TCSlackbot.Logic.Resources;
using TCSlackbot.Logic.Slack;
using TCSlackbot.Logic.Slack.Requests;
using TCSlackbot.Logic.TimeCockpit;
using TCSlackbot.Logic.Utils;
using System.Globalization;

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
        private readonly ITCManager _tcDataManager;
        private readonly CommandHandler _commandHandler;

        public ModalController(
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretManager.GetSecret("Slack-SlackbotOAuthAccessToken"));
            _tokenManager = tokenManager;
            _tcDataManager = dataManager;

            _commandHandler = new CommandHandler(_protector, _cosmosManager, _secretManager, _tokenManager, _tcDataManager);
        }

        /// <summary>
        /// Returns the data that is needed for the 'external select'.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("data")]
        public async Task<ContentResult> GetExternalData()
        {
            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            if (payload is null || payload.User is null)
            {
                return Content("");
            }

            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync(payload.User.Id);
                if (accessToken == null)
                {
                    return Content("");
                }

                // Create the list of projects
                string json = "{\"options\": [";
                var projects = await _tcDataManager.GetFilteredProjects(accessToken, payload.Value);
                foreach (var project in projects)
                {
                    json += "{\"text\": {\"type\": \"plain_text\",  \"text\": \"" + project.ProjectName + "\"},\"value\": \"" + project.ProjectName + "\" },";
                }

                json = json.Remove(json.Length - 1) + "]}";
                Console.WriteLine(json);
                return Content(json, "application/json");
            }
            catch (LoggedOutException)
            {
                return Content(BotResponses.ErrorLoggedOut);
            }
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

            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            if (payload is null || payload.User is null)
            {
                return BadRequest();
            }

            //
            // Ignore unecessary requests
            //
            if (payload.Type == "block_actions" || payload.Type == "view_closed")
                return Ok();

            //
            // Check if user is logged in, working
            //
            var user = await _commandHandler.GetSlackUserAsync(payload.User.Id);
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

        /// <summary>
        /// Called whenever the user wants to open a modal.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<IActionResult> ViewModalAsync(AppActionPayload payload)
        {
            if (payload is null)
            {
                return BadRequest();
            }

            var payloadUserId = payload?.User?.Id;
            var payloadTriggerId = payload?.TriggerId;
            var payloadCallbackId = payload?.CallbackId;
            if (payloadUserId is null || payloadTriggerId is null || payloadCallbackId is null)
            {
                return BadRequest();
            }

            var user = await _commandHandler.GetSlackUserAsync(payloadUserId);
            if (user == null)
            {
                return BadRequest();
            }

            //
            // Load the modal from the file
            //
            // TODO: Add a placeholder for this too (like below)
            string json = "{\"trigger_id\": \"" + payloadTriggerId + "\", \"view\": { \"type\": \"modal\", \"callback_id\": \"" + payloadCallbackId + "\",";
            json += await System.IO.File.ReadAllTextAsync("Json/StopModal.json");

            //
            // Set the initial values by replacing the placeholders in the json
            //
            var date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            json = json.Replace("REPLACE_DATE", "\"initial_date\": \"" + date + "\"", StringComparison.Ordinal);

            var startTime = user.StartTime.HasValue ? user.StartTime.Value.ToString("HH:mm", CultureInfo.InvariantCulture) : "";
            json = json.Replace("REPLACE_START", "\"initial_value\": \"" + startTime + "\"", StringComparison.Ordinal);


            var endTime = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
            json = json.Replace("REPLACE_END", "\"initial_value\": \"" + endTime + "\"", StringComparison.Ordinal);


            var projectName = user.DefaultProject != null ? user.DefaultProject.ProjectName : string.Empty;
            json = json.Replace("REPLACE_PROJECT", "\" " + projectName + "\"", StringComparison.Ordinal);

            //
            // Send the response
            //
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "views.open"), content);
            }

            return Ok(json);
        }

        public async Task<IActionResult> ProcessModalDataAsync(SlackUser user)
        {
            if (user is null)
            {
                return BadRequest();
            }

            var payload = Serializer.Deserialize<SlackViewSubmission>(HttpContext.Request.Form["payload"]);
            var payloadStart = payload?.View?.State?.Values?.Starttime?.StartTime?.Value;
            var payloadEnd = payload?.View?.State?.Values?.Endtime?.EndTime?.Value;
            var payloadDate = payload?.View?.State?.Values?.Date?.Date?.Day;
            var payloadUserId = payload?.User?.Id;
            var payloadDescription = payload?.View?.State?.Values?.Description?.Description?.Value;
            var payloadProjectName = payload?.View?.State?.Values?.Project?.Project?.Value;
            if (payloadEnd is null || payloadStart is null || payloadDate is null || payloadUserId is null || payloadDescription is null || payloadProjectName is null)
            {
                return BadRequest();
            }

            //
            // Generate the error response (if there are any)
            //
            string errorMessage = "{ \"response_action\": \"errors\", \"errors\": {";
            if (!TimeSpan.TryParseExact(payloadStart, "h\\:mm", CultureInfo.InvariantCulture, out TimeSpan startTime))
            {
                errorMessage += "\"starttime\": \"Please use a valid time format! (eg. '08:00')\",";
            }
            if (!TimeSpan.TryParseExact(payloadEnd, "h\\:mm", CultureInfo.InvariantCulture, out TimeSpan endTime))
            {
                errorMessage += "\"endtime\": \"Please use a valid time format! (eg. '18:00')\",";
            }
            if (payload?.View?.State?.Values?.Project == null)
            {
                errorMessage += "\"project\": \"Please select a project!\",";
            }
            else if (endTime.CompareTo(startTime) != 1)
            {
                errorMessage += "\"endtime\": \"End Time has to be after Start Time!\",";
            }

            //
            // Check if there was an error and return it
            //
            if (errorMessage.EndsWith(",", StringComparison.CurrentCulture))
            {
                errorMessage += "}}";

                // TODO: Check if Content works with IActionResult
                return Content(errorMessage, "application/json");
            }

            // 
            // 
            // 
            user.StartTime = payloadDate + startTime;
            user.EndTime = payloadDate + endTime;

            // 
            // Request the access token
            // 
            var accessToken = await _tokenManager.GetAccessTokenAsync(payloadUserId);
            if (accessToken == null)
            {
                return BadRequest();
            }

            //
            // Set the values
            //
            user.DefaultProject = await _tcDataManager.GetProjectAsync(accessToken, payloadProjectName);
            user.Description = payloadDescription;

            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);
            

            //
            // Send the reply
            //
            var channel = await CommandController.GetIMChannelFromUserAsync(_httpClient, payloadUserId);
            if (channel is null)
            {
                return BadRequest();
            }

            var replyData = new Dictionary<string, string>
            {
                ["user"] = payloadUserId,
                ["channel"] = channel,
                ["text"] = "Your time has been saved"
            };

            using var replyForm = new FormUrlEncodedContent(replyData);
            _ = await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "chat.postEphemeral"), replyForm);

            //
            // Update the user data (stop working, reset breaks)
            //
            user.IsWorking = false;
            user.ResetWorktime();

            // TODO: Reset breaks
            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            return Ok();
        }
    }
}