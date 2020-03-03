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

        [HttpPost]
        [Route("getData")]
        public IActionResult GetExternalDataTemp()
        {
            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);

            var debugData = new
            {
                options = new[]
                {
                    new {
                        text = new {
                            type = "plain_text",
                            text = " *this is plain_text text*"
                        },
                        value = "value-0"
                    }
                }
            };

            var data = JsonSerializer.Serialize(debugData);
            Console.WriteLine(data);

            using var content = new StringContent(data, Encoding.UTF8, "application/json");
            return Ok(content);
        }

        /// <summary>
        /// Send Project Data to Modal
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[Route("getData")]
        public async Task<IActionResult> GetExternalData()
        {
            var payload = Serializer.Deserialize<AppActionPayload>(HttpContext.Request.Form["payload"]);
            string json = "{\"options\": [";
            var queryData = new TCQueryData($"From P In Project Where P.Code Like '%{payload.Value}%' Select P");

            try
            {
                var accessToken = await _tokenManager.GetAccessTokenAsync(payload.User.Id);
                if (accessToken != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var projectList = await _httpClient.GetAsync("https://web.timecockpit.com/odata/APP_Project");
                    foreach (var i in JsonSerializer.Deserialize<ProjectRequest>(await projectList.Content.ReadAsStringAsync()).Values)
                    {
                        json += "{\"text\": {\"type\": \"plain_text\",  \"text\": \"" + i.ProjectName + "\"},\"value\": \"" + i.ProjectName + "\" },";
                    }

                    json = json.Remove(json.Length - 1) + "]}";
                    return Ok(new StringContent(json, Encoding.UTF8, "application/json"));
                }

            }
            catch (LoggedOutException)
            {
                return Ok(BotResponses.ErrorLoggedOut);
            }

            return BadRequest();
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

        public async Task<IActionResult> ViewModalAsync(AppActionPayload payload)
        {
            if (payload is null)
            {
                return BadRequest();
            }

            string json = "{\"trigger_id\": \"" + payload.TriggerId + "\", \"view\": { \"type\": \"modal\", \"callback_id\": \"" + payload.CallbackId + "\",";
            json += await System.IO.File.ReadAllTextAsync("Json/StopTimeTrackingNewTest.json"); // Changed to New

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "views.open"), content);
            }

            return Ok(json);
        }


        public async Task<IActionResult> ProcessModalDataAsync(SlackUser user)   /* , Dictionary<string,string> replyData */
        {
            if (user is null)
            {
                return BadRequest();
            }

            var payload = Serializer.Deserialize<SlackViewSubmission>(HttpContext.Request.Form["payload"]);

            string errorMessage = "{ \"response_action\": \"errors\", \"errors\": {";
            if (!TimeSpan.TryParse(payload.View.State.Values.Starttime.StartTime.Value, out TimeSpan startTime))
            {
                errorMessage += "\"starttime\": \"Please use a valid time format! (eg. \"08:00\")\",";
            }
            if (!TimeSpan.TryParse(payload.View.State.Values.Endtime.EndTime.Value, out TimeSpan endTime))
            {
                // TODO: send message to user
                errorMessage += "\"endtime\": \"Please use a valid time format! (eg. \"18:00\")\",";
            }
            if (endTime.CompareTo(startTime) != 1)
            {
                errorMessage += "\"endtime\": \"End Time has to be after Start Time!";
            }
            if (errorMessage.EndsWith(",", StringComparison.CurrentCulture))
            {
                errorMessage = errorMessage[0..^1] + "}}";

                /*
                var replyData = new Dictionary<string, string>();
                replyData["user"] = payload.User.Id;
                replyData["text"] = errorMessage;

                await _httpClient.PostAsync("https://747773f7.ngrok.io/modal", new FormUrlEncodedContent(replyData));
                */

                using (var content = new StringContent(errorMessage, Encoding.UTF8, "application/json"))
                {
                    await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "views.open"), content);
                }
                return BadRequest(errorMessage);


            }

            DateTime date = payload.View.State.Values.Date.Date.Day;

            user.StartTime = date + startTime;
            user.EndTime = date + endTime;
            if (payload.View.State.Values.Project == null)
            {
                user.Project = "";
            }
            else
            {
                user.Project = payload.View.State.Values.Project.Project.Value;
            }
            user.Description = payload.View.State.Values.Description.Description.Value;

            await _cosmosManager.ReplaceDocumentAsync(Collection.Users, user, user.UserId);

            user.ResetWorktime();


            var channel = await CommandController.GetIMChannelFromUserAsync(_httpClient, payload.User.Id);
            if (channel is null)
            {
                // TODO: Maybe return an error message?
                return BadRequest();
            }

            var replyData = new Dictionary<string, string>
            {
                ["user"] = payload.User.Id,
                ["channel"] = channel,
                ["text"] = "Your time has been saved"
            };

            _ = await _httpClient.PostAsync(new Uri(_httpClient.BaseAddress, "chat.postEphemeral"), new FormUrlEncodedContent(replyData));

            return Ok();
        }
    }
}