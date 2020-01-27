using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TCSlackbot.Logic
{
    public class Break
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

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
        /// The description of the working session.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The list of breaks during a working session. 
        /// </summary>
        public Stack<Break> Breaks { get; set; }

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
                    // TODO: Use this again
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
            // Check ALL breaks (you could theoretically also only check the latest one)
            get => Breaks.Any(b => b.End is null);
        }

        public bool StartBreak(DateTime date)
        {
            // 1. Get the top of the stack
            var @break = Breaks.Peek();

            // 2. Check if the end is set (break has ended)
            if (@break.Start is null || @break.End is null)
            {
                return false;
            }

            // 3. Insert a new break
            var newBreak = new Break
            {
                Start = date,
                End = null
            };
            Breaks.Push(newBreak);

            return true;
        }

        public bool StopBreak(DateTime date)
        {
            // 1. Get the top of the stack 
            var @break = Breaks.Peek();

            // 2. Check if the end is null (break has not ended)
            if (@break.End is null)
            {
                return false;
            }

            // 3. Set the end 
            @break.End = date;

            return true;
        }
    }
}
