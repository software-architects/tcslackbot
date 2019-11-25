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
            commandHandler = new CommandHandler(_cosmosManager, _secretManager);
            _httpClient = factory.CreateClient("BotClient");
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
                case "start": reply["text"] = commandHandler.StartWorktime(request); break;
                case "pause": case "break": reply["text"] = commandHandler.PauseWorktime(request); break;
                case "resume": reply["text"] = commandHandler.ResumeWorktime(request); break;
                case "starttime": case "gettime": reply["text"] = commandHandler.GetWorktime(request); break;
                default: break;
            }

            await SendPostRequest(reply, secret);
            return Ok("Worked");
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
