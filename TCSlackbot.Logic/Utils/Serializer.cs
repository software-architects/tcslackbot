using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace TCSlackbot.Logic.Utils
{
    public static class Serializer
    {
        /// <summary>
        /// Deserializes the specified content to the specified type (with specific serialization options).
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data</typeparam>
        /// <param name="content">The serialized content</param>
        /// <returns>The deserialized object of the specified type</returns>
        public static T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
