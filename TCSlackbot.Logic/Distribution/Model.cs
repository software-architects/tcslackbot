using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCSlackbot.Logic.Distribution
{
    public class Bot
    {
        [JsonPropertyName("bot_user_id")]
        public string BotUserId { get; set; } = string.Empty;

        [JsonPropertyName("bot_access_token")]
        public string BotAccessToken { get; set; } = string.Empty;
    }

    public class DistributionResponse
    {
        /// <summary>
        ///  Flag whether the request was successful. 
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// The error message if Ok is false.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// The unique identifier of the team.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// The information about the added bot.
        /// </summary>
        public Bot? Bot { get; set; }
    }
}
