using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public partial class ConversationsList
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("channels")]
        public List<Channel> Channels { get; set; }
    }

    public partial class Channel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}
