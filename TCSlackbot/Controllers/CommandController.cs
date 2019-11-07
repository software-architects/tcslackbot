﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
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
        private readonly IDataProtector _protector;

        public CommandController(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("UUIDProtector");
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
        public IActionResult Login([FromForm] SlackSlashCommand ssc)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return Ok("https://ngroksomething.com/auth/link/?uuid=" + _protector.Protect(ssc.UserId));
            } else
            {
                return Ok("https://tcslackbot.azurewebsites.net:6001/auth/link/?" + _protector.Protect(ssc.UserId));
            }
            
        }
    }
}
