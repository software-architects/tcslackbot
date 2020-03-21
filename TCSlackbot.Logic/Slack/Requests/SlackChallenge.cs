using TCSlackbot.Logic.Slack;

namespace TCSlackbot.Logic
{
    public class SlackChallenge : SlackBaseRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Challenge { get; set; } = string.Empty;
    }
}
