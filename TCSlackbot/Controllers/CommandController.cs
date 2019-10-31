using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCSlackbot.Logic;

namespace TCSlackbot.Controllers
{   

    [ApiController]
    [Route("command")]
    public class CommandController : ControllerBase
    {
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
    }
}
