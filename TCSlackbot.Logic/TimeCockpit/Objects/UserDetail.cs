using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.TimeCockpit.Objects
{
    public class UserDetail
    {
        [JsonPropertyName("APP_UserDetailUuid")]
        public string? UserDetailUuid { get; set; }
    }
}
