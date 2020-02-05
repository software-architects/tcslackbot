using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public partial class ProjectRequest
    {
        // [JsonPropertyName("ok")]
        // public bool Ok { get; set; }

        [JsonPropertyName("value")]
        public List<ProjectValues> Values { get; set; }
    }

    public partial class ProjectValues
    {
        [JsonPropertyName("APP_Code")]
        public string ProjectName { get; set; }

        // [JsonPropertyName("user")]
        // public string User { get; set; }
    }
}
