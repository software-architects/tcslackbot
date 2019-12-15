using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class AppActionPayload
    {
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; }
        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
