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
        [JsonPropertyName("channel")]
        public UserChannel Channel { get; set; }
        [JsonPropertyName("user")]
        public UserChannel User { get; set; }
    }

    //User and Channel have the same properties
    public class UserChannel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
