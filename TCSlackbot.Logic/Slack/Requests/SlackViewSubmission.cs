using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public partial class SlackViewSubmission
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; }

        [JsonPropertyName("view")]
        public View View { get; set; }
    }

    public partial class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public partial class View
    {
        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; }

        [JsonPropertyName("state")]
        public State State { get; set; }
    }

    public partial class State
    {  
        [JsonPropertyName("values")]
        public Values Values { get; set; }
    }

    public partial class Values
    {
        // Date, StartTime, EndTime, Description
        // I dont know what JsonPropertyName i should put here as the string is random
        // See TestViewSubmission.json
        public Dictionary<string, IdentifierString> AllStrings { get; set; }
    }

    public partial class IdentifierString
    {
        public Dictionary<string, Details> AllValues { get; set; }
    }

    public partial class Details
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("selected_date")]
        public string Date { get; set; }
    }
}
