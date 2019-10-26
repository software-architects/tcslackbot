using System.Threading.Tasks;

namespace TCSlackbot.Logic
{
    public interface IBotClient
    {
        Task Test(SlackConfig config);
    }
}
