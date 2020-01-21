using Newtonsoft.Json;
using System;

namespace TCSlackbot.Logic
{
    public class SlackUser
    {
        /// <summary>
        /// The id of the user (and document). 
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; }

        /// <summary>
        /// The id of the user (and document). 
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// The start time of the working session.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// The end time of the working session.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// The end time of the working session.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The total break time in minutes.
        /// </summary>
        public decimal TotalBreakTime { get; set; } = 0;

        /// <summary>
        /// The time the user went on break
        /// </summary>
        public DateTime? BreakTime { get; set; }

        /// <summary>
        /// Boolean whether the user is working.
        /// </summary>
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
                    // StartTime = null;
                    // EndTime = null;
                }
            }
        }

        /// <summary>
        /// Boolean whether the user is on break.
        /// </summary>
        [JsonIgnore]
        public bool IsOnBreak
        {
            get => !(BreakTime is null);
        }

    }
}
