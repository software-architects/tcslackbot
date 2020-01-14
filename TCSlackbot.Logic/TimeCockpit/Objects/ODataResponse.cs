using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public T[] Value { get; set; }
    }
}
