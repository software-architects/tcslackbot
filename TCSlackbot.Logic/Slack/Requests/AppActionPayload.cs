using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class AppActionPayload
    {
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
