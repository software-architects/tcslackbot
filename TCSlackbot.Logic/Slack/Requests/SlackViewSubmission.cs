using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCSlackbot.Logic.Slack.Requests
{
    public partial class SlackViewSubmission
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public SlackUser User { get; set; }

        [JsonProperty("view")]
        public View View { get; set; }
    }

    public partial class View
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("private_metadata")]
        public string PrivateMetadata { get; set; }

        [JsonProperty("callback_id")]
        public string CallbackId { get; set; }

        [JsonProperty("state")]
        public State State { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public partial class State
    {
        [JsonProperty("values")]
        public Values Values { get; set; }
    }

    public partial class Values
    {
        [JsonProperty("multi-line")]
        public MultiLine MultiLine { get; set; }
    }

    public partial class MultiLine
    {
        [JsonProperty("ml-value")]
        public MlValue MlValue { get; set; }
    }

    public partial class MlValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
