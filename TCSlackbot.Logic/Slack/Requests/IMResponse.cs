﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Slack.Requests
{
    public class IMResponse
    {
        public partial class Payload
        {
            [JsonPropertyName("ok")]
            public bool Ok { get; set; }
            [JsonPropertyName("ims")]
            public List<InstantMessage> Ims { get; set; }
        }

        public partial class InstantMessage
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("user")]
            public string User { get; set; }
        }
    }
}
