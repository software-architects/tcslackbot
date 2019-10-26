using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TCSlackbot.Controllers {
    [ApiController]
    [Route("tcslackbot")]
    public class SlackController : ControllerBase {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SlackController> _logger;

        public SlackController(ILogger<SlackController> logger) {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Login() {
            return Ok(Summaries);   
        }
    }
}
