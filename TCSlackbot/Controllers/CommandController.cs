using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TCSlackbot.Logic;
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

        public CommandController(IDataProtectionProvider provider, ISecretManager secretManager, ICosmosManager cosmosManager, IHttpClientFactory factory)
        {
            _protector = provider.CreateProtector("UUIDProtector");
            _secretManager = secretManager;
            _cosmosManager = cosmosManager;
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://slack.com/api/");
        }
        /* Do not delete
        [HttpPost]
        public IActionResult CheckChallenge([FromBody]SlackChallengeToken sct)
        {
            Console.WriteLine("test");
            return Ok(sct.Challenge);
        }
        */
        [HttpPost]
        public async Task<IActionResult> HandleIncomingSlackRequest([FromBody]SlackRequest request)
        {
            if (request.Event.Type == "message")
            {
                var reply = new Dictionary<string, string>();
                bool secret = false; // If secret is true then a ephemeral message will be 
                                     // sent, which can only be seen by the one who wrote the message

                reply["user"] = request.Event.User;
                reply["token"] = _secretManager.GetSecret("Slack-SlackbotOAuthAccessToken");
                reply["channel"] = request.Event.Channel;
                //reply["attachments"] = "[{\"fallback\":\"dummy\", \"text\":\"this is an attachment\"}]";

                switch (request.Event.Text.ToLower())
                {
                    case "login": reply["text"] = LoginEventsAPI(request); secret = true; break;
                    case "link": reply["text"] = LoginEventsAPI(request); secret = true; break;
                    case "start": reply["text"] = StartWorktime(request); break;
                    case "pause": reply["text"] = PauseWorktime(request); break;
                    default: break;
                }
                await SendPostRequest(reply, secret);
                return Ok("Worked");
            }
            return Ok();
        }

        private string PauseWorktime(SlackRequest request)
        {
            if (IsLoggedIn(request) && IsWorking(request) && !IsOnBreak(request))
            {
                // Somehow insert into the db that the user is on break
                return "Break has been set. You can now relax.";
            }
            if (!IsLoggedIn(request)) return "You have to login before you can use this bot!\nType login or link to get the login link.";
            if (!IsWorking(request)) return "You are not working at the moment. Did you forget to type start?";
            if (IsOnBreak(request)) return "You are already on break. Did you forget to unpause?";
            return "You shouldn't get this message.";
        }

        private string StartWorktime(SlackRequest request)
        {
            if (IsLoggedIn(request))
            {
                var user = new SlackUser()
                {
                    UserId = request.Event.User,
                    StartTime = DateTime.Now
                };
                    
                _cosmosManager.CreateDocumentAsync("Slack_users", user);
                return "StartTime has been set!";
            }
            return "You have to login before you can use this bot!\nType login or link to get the login link.";
        }

        private bool IsLoggedIn(SlackRequest request)
        {
            return _secretManager.GetSecret(request.Event.User) != null;
        }
        private bool IsWorking(SlackRequest request)
        {
            // Check in CosmosDB is user has start time set
            throw new NotImplementedException();
        }
        private bool IsOnBreak(SlackRequest request)
        {
            throw new NotImplementedException();

            //return _cosmosManager.GetDocumentAsync<SlackUser>()
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
        public string LoginEventsAPI(SlackRequest request)
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

        [HttpPost]
        [Route("status")]
        public JsonResult GetStatus() // [FromForm] SlackSlashCommand ssc
        {
            var dict = HttpContext.Request.Form;

            System.Console.WriteLine(dict["token"]);

            return new JsonResult(dict);
        }



    }
}

/*
 * Old Code
[HttpPost]
        [Route("slashcommand")]
        public JsonResult HandleCommand([FromForm] SlackSlashCommand ssc)
        {
            return new JsonResult("You did it.");
        }

        [HttpPost]
        [Route("ping")]
        public IActionResult Ping([FromForm] SlackSlashCommand ssc)
        {
            return Ok("Pong");
        }
 * 
 */
