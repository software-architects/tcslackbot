using System;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    public class Timesheet
    {
        [JsonPropertyName("APP_BeginTime")]
        public DateTime BeginTime { get; set; }

        [JsonPropertyName("APP_EndTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("APP_Description")]
        public string? Description { get; set; }

        [JsonPropertyName("APP_UserDetailUuid")]
        public string? UserDetailUuid { get; set; }

        [JsonPropertyName("APP_ProjectUuid")]
        public string? ProjectUuid { get; set; }
    }
}
