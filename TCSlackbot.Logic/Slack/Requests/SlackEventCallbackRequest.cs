using Newtonsoft.Json;

namespace TCSlackbot.Logic.Slack
{
    public class SlackEvent : SlackBaseRequest
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("ts")]
        public string Ts { get; set; }

        [JsonProperty("event_ts")]
        public string EventTs { get; set; }

        [JsonProperty("channel_type")]
        public string ChannelType { get; set; }
    }

    public class SlackEventCallbackRequest : SlackBaseRequest
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("team_id")]
        public string TeamId { get; set; }

        [JsonProperty("api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty("event")]
        public SlackEvent Event { get; set; }

        [JsonProperty("authed_teams")]
        public string[] AuthedTeams { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("event_time")]
        public long EventTime { get; set; }
    }
}
