using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    public class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T>? Value { get; }
    }
}
