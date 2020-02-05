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
        public Stack<Break>? Breaks { get; set; }

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
                    EndTime = DateTime.Now;
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
            get => !(Breaks is null) || Breaks.Any(b => b.End is null);
        }

        /// <summary>
        /// Resets the worktime (e.g. when the user stopped working).
        /// </summary>
        public void ResetWorktime()
        {
            StartTime = null;
            EndTime = null;
        }

        /// <summary>
        /// Starts a break and adds it to the list.
        /// </summary>
        /// <param name="date">The time, when the break started.</param>
        /// <returns>True if successful</returns>
        public bool StartBreak(DateTime date)
        {
            if (Breaks == null)
            {
                Breaks = new Stack<Break>();
            }

            if (Breaks.Count != 0)
            {
                // 1. Get the top of the stack
                var @break = Breaks.Peek();

                // 2. Check if there's already a break which has not been ended yet
                if (@break.End is null)
                {
                    return false;
                }
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

        /// <summary>
        /// Stops the current breaks.
        /// </summary>
        /// <param name="date">The time, when the break stopped.</param>
        /// <returns>True if successful</returns>
        public bool StopBreak(DateTime date)
        {
            if (Breaks == null)
            {
                return false;
            }

            // 1. Get the top of the stack 
            var @break = Breaks.Peek();

            // 2. Check if the there's an break which has not been ended yet
            if (@break.End != null)
            {
                return false;
            }

            // 3. Set the end 
            @break.End = date;

            return true;
        }
    }
}
