using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TCSlackbot.Logic.TimeCockpit.Objects;

namespace TCSlackbot.Logic
{
    public class Duration
    {

        public Duration(DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
        }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class SlackUser
    {
        /// <summary>
        /// The id of the user (and document). 
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Contains the start and end time of a working session. 
        /// </summary>
        public Duration? Worktime { get; set; }

        /// <summary>
        /// The list of breaks during a working session. 
        /// </summary>
        // TODO: CHECK IF THE LIST OF BREAKS WILL STILL BE RECEIVED FROM THE COSMOS DB (might not be working with this change)
        public Stack<Duration> Breaks { get; } = new Stack<Duration>();

        /// <summary>
        /// The default project, used whenever a user executes a command and doesn't pass a custom project.
        /// </summary>
        public Project? DefaultProject { get; set; }

        /// <summary>
        /// If set to true, it sets the start time to the current datetime. If set to false, it sets the end time to the current datetime. 
        /// </summary>
        [JsonIgnore]
        public bool IsWorking
        {
            
            get => !(Worktime?.Start is null);
            set
            {
                if (Worktime == null)
                {
                    Worktime = new Duration(default, default);
                }

                if (value is true)
                {
                    Worktime.Start = DateTime.Now;
                }
                else
                {
                    Worktime.End = DateTime.Now;
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
            get => !(Breaks is null) && Breaks.Any(b => b.End is null);
        }

        /// <summary>
        /// Resets the worktime (e.g. when the user stopped working).
        /// </summary>
        public void ResetWorktime()
        {
            Worktime = new Duration(default, default);
        }

        /// <summary>
        /// Starts a break and adds it to the list.
        /// </summary>
        /// <param name="date">The time, when the break started.</param>
        /// <returns>True if successful</returns>
        public bool StartBreak(DateTime date)
        {
            if (Breaks?.Count != 0)
            {
                // 1. Get the top of the stack
                var @break = Breaks.LastOrDefault();

                // 2. Check if there's already a break which has not been ended yet
                if (@break == null || @break.End is null)
                {
                    return false;
                }
            }

            // 3. Insert a new break
            var newBreak = new Duration(date, null);
            Breaks?.Push(newBreak);

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
            var @break = Breaks.LastOrDefault();

            // 2. Check if the there's an break which has not been ended yet
            if (@break == default || @break.End != null)
            {
                return false;
            }

            // 3. Set the end 
            @break.End = date;

            return true;
        }

        /// <summary>
        /// The total time spent on breaks.
        /// </summary>
        /// <returns>A time span with the total break time</returns>
        public TimeSpan TotalBreakTime() => new TimeSpan(Breaks.Sum(b =>
        {
            // If end has not been set, use current date
            if (b.End == null)
            {
                return (DateTime.Now - b.Start.GetValueOrDefault()).Ticks;
            }

            return (b.End.GetValueOrDefault() - b.Start.GetValueOrDefault()).Ticks;
        }));

        /// <summary>
        /// The total work time without breaks.
        /// </summary>
        /// <returns>A time span with the total work time. If the end has not been set, the current time is used.</returns>
        public TimeSpan TotalWorkTime() => 
            ((Worktime?.End == null ? DateTime.Now : Worktime.End.GetValueOrDefault()) - (Worktime?.Start == null ? DateTime.Now : Worktime.Start.GetValueOrDefault())) - TotalBreakTime();

        public IEnumerable<Duration> GetWorkSessions()
        {
            var sessions = new List<Duration>();
            var breaks = Breaks.OrderBy(duration => duration.Start);

            if(Worktime?.Start == null || Worktime?.End == null)
            {
                return sessions;
            }

            // Check if there are any breaks
            if (!breaks.Any())
            {
                sessions.Add(new Duration(Worktime.Start, Worktime.End));
                return sessions;
            }

            // Last session will be set after loop
            for (var i = 0; i <= breaks.Count() - 1; i++)
            {
                var curBreak = breaks.ElementAt(i);

                if (i == 0)
                {
                    sessions.Add(new Duration(Worktime.Start, curBreak.Start));
                } else
                {
                    var lastBreak = breaks.ElementAt(i - 1);

                    sessions.Add(new Duration(lastBreak.End, curBreak.Start));
                }

            }

            sessions.Add(new Duration(breaks.Last().End, Worktime.End));

            return sessions;
        }
    }
}
