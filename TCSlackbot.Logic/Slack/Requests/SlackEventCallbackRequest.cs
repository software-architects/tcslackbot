using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack
{
    public class SlackEvent : SlackBaseRequest
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public string Ts { get; set; } = string.Empty;

        [JsonPropertyName("event_ts")]
        public string EventTs { get; set; } = string.Empty;

        [JsonPropertyName("channel_type")]
        public string ChannelType { get; set; } = string.Empty;
    }

    public class SlackEventCallbackRequest : SlackBaseRequest
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        [JsonPropertyName("api_app_id")]
        public string ApiAppId { get; set; } = string.Empty;

        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = string.Empty;

        [JsonPropertyName("event_time")]
        public long EventTime { get; set; } = long.MinValue;

        [JsonPropertyName("event")]
        public SlackEvent? Event { get; set; }

        [JsonPropertyName("authed_teams")]
        public List<string> AuthedTeams { get; } = new List<string>();
    }
}
