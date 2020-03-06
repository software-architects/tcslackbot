using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    public class Project
    {
        [JsonPropertyName("APP_Code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("APP_ProjectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("APP_ProjectUuid")]
        public string ProjectUuid { get; set; } = string.Empty;
    }
}
