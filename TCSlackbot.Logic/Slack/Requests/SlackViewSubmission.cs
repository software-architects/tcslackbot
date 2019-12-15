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
        public SlackUser User { get; set; }

        [JsonPropertyName("view")]
        public View View { get; set; }
    }

    public partial class View
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("private_metadata")]
        public string PrivateMetadata { get; set; }

        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; }

        [JsonPropertyName("state")]
        public State State { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }

    public partial class State
    {
        [JsonPropertyName("values")]
        public Values Values { get; set; }
    }

    public partial class Values
    {
        [JsonPropertyName("multi-line")]
        public MultiLine MultiLine { get; set; }
    }

    public partial class MultiLine
    {
        [JsonPropertyName("ml-value")]
        public MlValue MlValue { get; set; }
    }

    public partial class MlValue
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
