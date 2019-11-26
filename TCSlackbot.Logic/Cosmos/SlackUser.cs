using System;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic
{
    public class SlackUser
    {
        [JsonPropertyName("id")]
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime BreakTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool OnBreak { get; set; } = false;
    }
}
