using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class AppActionPayload
    {
        [JsonProperty("trigger_id")]
        public int TriggerId { get; set; }
        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
