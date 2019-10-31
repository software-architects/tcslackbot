
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCSlackbot.Logic
{
    public sealed class SlackReply
    {

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        [JsonProperty("attachments")]
        public List<Dictionary<string, string>> Attachments { get; set; }

        /*
        public SlackReply(string text, List<Dictionary<string, string>> attachments)
        {
            Text = text;
            Attachments = attachments;
        }
        */
    }
}