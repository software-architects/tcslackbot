using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TCSlackbot.Logic;

namespace TCSlackbot.Controllers
{

    [ApiController]
    [Route("command")]
    public class CommandController : ControllerBase
    {
        private readonly IDataProtector _protector;

        public CommandController(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("UUIDProtector");
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
        public IActionResult Login([FromForm] SlackSlashCommand ssc)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return Ok("https://localhost:6001/auth/link/?uuid=" + _protector.Protect(ssc.Token)); // eigentlich ssc.UserId ist aber im moment null
            }
            else
            {
                return Ok("https://tcslackbot.azurewebsites.net:6001/auth/link/?" + _protector.Protect(ssc.UserId));
            }

        }
    }
}
