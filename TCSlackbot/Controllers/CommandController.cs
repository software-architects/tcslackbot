using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        private const string CollectionId = "slack_users";

        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;
        private readonly ICosmosManager _cosmosManager;
        private readonly HttpClient _httpClient;

        public CommandController(IDataProtectionProvider provider, ISecretManager secretManager, ICosmosManager cosmosManager, IHttpClientFactory factory)
        {
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://slack.com/api/");
        }

        [HttpPost]
        public async Task<IActionResult> HandleRequestAsync([FromBody] dynamic body)
        {
            var request = Deserialize<SlackBaseRequest>(body.ToString());

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

        public async Task<IActionResult> HandleEventCallbackAsync(SlackEventCallbackRequest request)
        {
            switch (request.Event.Type)
            {
                case "message":
                    // TODO: Only pass the event
                    return await HandleSlackMessage(request);

                default:
                    break;
            }

            return NotFound();
        }


        public IActionResult HandleSlackChallenge(SlackChallenge request)
        {
            return Ok(request.Challenge);
        }

        public async Task<IActionResult> HandleSlackMessage(SlackEventCallbackRequest request)
        {
            var reply = new Dictionary<string, string>();
            bool secret = false; // If secret is true then a ephemeral message will be 
                                 // sent, which can only be seen by the one who wrote the message

            reply["user"] = request.Event.User;
            reply["token"] = _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken");
            reply["channel"] = request.Event.Channel;
            //reply["attachments"] = "[{\"fallback\":\"dummy\", \"text\":\"this is an attachment\"}]";

            switch (request.Event.Text.ToLower().Trim())
            {
                case "login": case "link": reply["text"] = LoginEventsAPI(request); secret = true; break;
                case "start": reply["text"] = StartWorktime(request); break;
                case "pause": case "break": reply["text"] = PauseWorktime(request); break;
                case "resume": reply["text"] = ResumeWorktime(request); break;
                case "starttime": case "get time": reply["text"] = ResumeWorktime(request); break;
                default: break;
            }

            await SendPostRequest(reply, secret);
            return Ok("Worked");
        }

        //TODO
        private string ResumeWorktime(SlackEventCallbackRequest request)
        {
            var curBreakTime = _cosmosManager.GetSlackUser("Slack_users", request.Event.User).BreakTime;
            return "Started at: " + _cosmosManager.GetSlackUser("Slack_users", request.Event.User).BreakTime;
        }

        private string PauseWorktime(SlackEventCallbackRequest request)
        {
            if (IsLoggedIn(request) && IsWorking(request) && !IsOnBreak(request))
            {
                var curBreakTime = _cosmosManager.GetSlackUser("Slack_users", request.Event.User).OnBreak;

                return "Break has been set. You can now relax.";
            }
            if (!IsLoggedIn(request)) return "You have to login before you can use this bot!\nType login or link to get the login link.";
            if (!IsWorking(request)) return "You are not working at the moment. Did you forget to type start?";
            if (IsOnBreak(request)) return "You are already on break. Did you forget to unpause?";
            return "You shouldn't get this message.";
        }

        private string StartWorktime(SlackEventCallbackRequest request)
        {
            if (IsLoggedIn(request))
            {
                var user = new SlackUser()
                {
                    UserId = request.Event.User,
                    StartTime = DateTime.Now
                };

                _cosmosManager.CreateDocumentAsync(CollectionId, user);
                return "StartTime has been set!";
            }
            return "You have to login before you can use this bot!\nType login or link to get the login link.";
        }

        private bool IsLoggedIn(SlackEventCallbackRequest request)
        {
            return _secretManager.GetSecret(request.Event.User) != null;
        }
        private bool IsWorking(SlackEventCallbackRequest request)
        {
            return _cosmosManager.GetSlackUser("Slack_users", request.Event.User).StartTime != null;
        }
        private bool IsOnBreak(SlackEventCallbackRequest request)
        {
            return _cosmosManager.GetSlackUser("Slack_users", request.Event.User).OnBreak;
        }

        private async Task SendPostRequest(Dictionary<string, string> dict, bool secret)
        {
            if (secret)
            {
                await _httpClient.PostAsync("chat.postEphemeral", new FormUrlEncodedContent(dict));
            }
            else
            {
                await _httpClient.PostAsync("chat.postMessage", new FormUrlEncodedContent(dict));
            }
        }

        [NonAction]
        public string LoginEventsAPI(SlackEventCallbackRequest request)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return "<https://localhost:6001/auth/link/?uuid=" + _protector.Protect(request.Event.User) + "|Link TimeCockpit Account>";
            }
            else
            {
                return "<https://tcslackbot.azurewebsites.net/auth/link/?uuid=" + _protector.Protect(request.Event.User) + "|Link TimeCockpit Account>";
            }
        }

        private static T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
