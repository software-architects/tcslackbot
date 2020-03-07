using System;

namespace TCSlackbot.Logic.Authentication
{
    public class LinkData
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
    }
}
