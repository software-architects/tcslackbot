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
        public string? Type { get; set; }

        [JsonPropertyName("user")]
        public User? User { get; set; }

        [JsonPropertyName("trigger_id")]
        public string? TriggerId { get; set; }

        [JsonPropertyName("view")]
        public View? View { get; set; }
    }

    public partial class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public partial class View
    {
        [JsonPropertyName("callback_id")]
        public string? CallbackId { get; set; }

        [JsonPropertyName("state")]
        public State? State { get; set; }
    }

    public partial class State
    {  
        [JsonPropertyName("values")]
        public Values? Values { get; set; }
    }

    public partial class Values
    {
        [JsonPropertyName("date")]
        public DateClass? Date { get; set; }

        [JsonPropertyName("starttime")]
        public Starttime? Starttime { get; set; }

        [JsonPropertyName("endtime")]
        public Endtime? Endtime { get; set; }

        [JsonPropertyName("description")]
        public DescriptionClass? Description { get; set; }

        [JsonPropertyName("project")]
        public ProjectClass? Project { get; set; }
    }

    public partial class DateClass
    {
        [JsonPropertyName("Date")]
        public Details? Date { get; set; }
    }

    public partial class DescriptionClass
    {
        [JsonPropertyName("Description")]
        public Details? Description { get; set; }
    }
    public partial class ProjectClass
    {
        [JsonPropertyName("Date")]
        public Details? Project { get; set; }
    }


    public partial class Details
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("selected_date")]
        public DateTime? Day { get; set; }
    }

    public partial class Endtime
    {
        [JsonPropertyName("EndTime")]
        public Details? EndTime { get; set; }
    }

    public partial class Starttime
    {
        [JsonPropertyName("StartTime")]
        public Details? StartTime { get; set; }
    }
}
