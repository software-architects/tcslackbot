using System.Text.Json;

namespace TCSlackbot.Logic.TimeCockpit
{
    public class TCNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) =>
            $"APP_{name}";
    }
}
