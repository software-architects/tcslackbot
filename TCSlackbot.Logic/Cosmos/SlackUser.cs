﻿using Newtonsoft.Json;
using System;

namespace TCSlackbot.Logic
{
    public class SlackUser
    {
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }


        public decimal TotalBreakTime { get; set; } //In Minutes

        [JsonIgnore]
        public bool IsWorking
        {
            get => !(StartTime is null);
            set
            {
                if (value is true)
                {
                    StartTime = DateTime.Now;
                }
                else
                {
                    StartTime = null;
                    EndTime = null;
                }
            }
        }
        [JsonIgnore]
        public DateTime? BreakTime { get; set; }
        [JsonIgnore]
        public bool IsOnBreak
        {
            get => !(BreakTime is null);
        }

    }
}
