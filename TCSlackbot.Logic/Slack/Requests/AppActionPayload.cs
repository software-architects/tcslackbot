using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class AppActionPayload
    {
        [JsonProperty("trigger_id")]
        public int TriggerId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
