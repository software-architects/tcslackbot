using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack
{
    public class SlackEvent : SlackBaseRequest
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("ts")]
        public string Ts { get; set; }

        [JsonPropertyName("event_ts")]
        public string EventTs { get; set; }

        [JsonPropertyName("channel_type")]
        public string ChannelType { get; set; }
    }

    public class SlackEventCallbackRequest : SlackBaseRequest
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("team_id")]
        public string TeamId { get; set; }

        [JsonPropertyName("api_app_id")]
        public string ApiAppId { get; set; }

        [JsonPropertyName("event")]
        public SlackEvent Event { get; set; }

        [JsonPropertyName("authed_teams")]
        public List<string> AuthedTeams { get; } = new List<string>();

        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        [JsonPropertyName("event_time")]
        public long EventTime { get; set; }
    }
}
