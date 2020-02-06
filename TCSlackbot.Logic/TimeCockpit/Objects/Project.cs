using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    public class Project
    {
        [JsonPropertyName("APP_Code")]
        public string Code { get; set; }

        [JsonPropertyName("APP_ProjectName")]
        public string ProjectName { get; set; }

        [JsonPropertyName("APP_ProjectUuid")]
        public string ProjectUuid { get; set; }

        // TODO: Add more
    }
}
