using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class SlackViewSubmission
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("user")]
        public User? User { get; set; }

        [JsonPropertyName("team")]
        public Team? Team { get; set; }

        [JsonPropertyName("trigger_id")]
        public string? TriggerId { get; set; }

        [JsonPropertyName("view")]
        public View? View { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class View
    {
        [JsonPropertyName("callback_id")]
        public string? CallbackId { get; set; }

        [JsonPropertyName("state")]
        public State? State { get; set; }
    }

    public class State
    {
        [JsonPropertyName("values")]
        public Values? Values { get; set; }
    }

    public class Values
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

    public class DateClass
    {
        [JsonPropertyName("Date")]
        public DateDetails? Date { get; set; }
    }

    public class DescriptionClass
    {
        [JsonPropertyName("Description")]
        public DateDetails? Description { get; set; }
    }

    public class ProjectClass
    {
        [JsonPropertyName("Project")]
        public ProjectDetails? Project { get; set; }
    }

    public class DateDetails
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("selected_date")]
        public DateTime? Day { get; set; }
    }

    public class ProjectDetails
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("selected_option")]
        public SelectedOption? SelectedOption { get; set; }
    }

    public class Endtime
    {
        [JsonPropertyName("EndTime")]
        public DateDetails? EndTime { get; set; }
    }

    public class Starttime
    {
        [JsonPropertyName("StartTime")]
        public DateDetails? StartTime { get; set; }
    }

    public class SelectedOption
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

}
