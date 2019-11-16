using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TCSlackbot.Logic;

namespace TCSlackbot.Controllers
{

    [ApiController]
    [Route("command")]
    public class CommandController : ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly ISecretManager _secretManager;
        private readonly HttpClient _httpClient;

        public CommandController(IDataProtectionProvider provider, ISecretManager secretManager, IHttpClientFactory factory)
        {
            _secretManager = secretManager;
            _protector = provider.CreateProtector("UUIDProtector");
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://slack.com/api/");
        }
        /*
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
            var reply = new NameValueCollection();
            bool secret = false; // If secret is true then a ephemeral message will be 
                                 // sent, which can only be seen by the one who wrote the message

            reply["user"] = request.Event.User;
            reply["token"] = _secretManager.GetSecret("Slack-OAuthAccessToken");
            reply["channel"] = request.Event.Channel;
            //reply["attachments"] = "[{\"fallback\":\"dummy\", \"text\":\"this is an attachment\"}]";

            switch (request.Event.Text)
            {
                case "login": reply["text"] = LoginEventsAPI(request); secret = true; break;
                case "link": reply["text"] = LoginEventsAPI(request); secret = true; break;
                default: break;
            }
            await SendPostRequest(reply, secret);
            return Ok("Worked");
        }

        private async Task SendPostRequest(NameValueCollection reply, bool secret)
        {
            var dict = reply.AllKeys.ToDictionary(t => t, t => reply[t]);
            if (secret)
            {
                await _httpClient.PostAsync("chat.postEphemeral", new FormUrlEncodedContent(dict)); //doesnt work yet i think
            }
            else
            {
                await _httpClient.PostAsync("chat.postMessage", new FormUrlEncodedContent(dict));
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

        [HttpPost]
        [Route("login")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Login(SlackRequest request) //[FromForm] SlackSlashCommand ssc
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return Ok("https://localhost:6001/auth/link/?uuid=" + _protector.Protect(request.Event.User));
            }
            else
            {
                return Ok("https://tcslackbot.azurewebsites.net/auth/link/?uuid=" + _protector.Protect(request.Event.User));
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
    }
}
