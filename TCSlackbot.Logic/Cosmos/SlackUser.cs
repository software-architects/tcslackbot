using Newtonsoft.Json;
using System;

namespace TCSlackbot.Logic
{
    public class SlackUser
    {
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime BreakTime { get; set; }

        public bool IsOnBreak { get; set; } = false;

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
    }
}
