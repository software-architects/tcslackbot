using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class AppActionPayload
    {
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; } = string.Empty;

        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("channel")]
        public UserChannel? Channel { get; set; }

        [JsonPropertyName("user")]
        public UserChannel? User { get; set; }

        [JsonPropertyName("Value")]
        public string Value { get; set; } = string.Empty;
    }

    //User and Channel have the same properties
    public class UserChannel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
