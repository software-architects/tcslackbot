using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCSlackbot.Logic
{
    public class SlackChallengeToken
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("challenge")]
        public string Challenge { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
