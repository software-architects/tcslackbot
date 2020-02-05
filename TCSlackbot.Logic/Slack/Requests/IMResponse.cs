using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public static class IMResponse
    {
        private class Payload
        {
            [JsonPropertyName("ok")]
            public bool Ok { get; set; }

            [JsonPropertyName("ims")]
            public List<InstantMessage> Ims { get; set; } = new List<InstantMessage>();
        }

        private class InstantMessage
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("user")]
            public string User { get; set; }
        }
    }
}
