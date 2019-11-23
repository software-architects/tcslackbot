using TCSlackbot.Logic.Slack;

namespace TCSlackbot.Logic
{
    public class SlackChallenge : SlackBaseRequest
    {
        public string Token { get; set; }
        public string Challenge { get; set; }
    }
}
