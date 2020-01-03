using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic.TimeCockpit;

namespace TCSlackbot.Logic.Utils
{
    public class TCManager : ITCDataManager
    {
        private static readonly HttpClient client = new HttpClient();


        public async Task<IEnumerable<T>> GetFilteredObjectsAsync<T>(string accessToken, string filter)
        {
            var objectName = $"APP_{typeof(T).Name}";

            Console.WriteLine(objectName);

            return default;
        }

        public async Task<IEnumerable<T>> GetObjectsAsync<T>(string accessToken)
        {
            // TODO:
            // Get the object name
            var objectName = $"APP_{typeof(T).Name}";

            // Send request to: 
            var response = await client.GetStringAsync($"https://apipreview.timecockpit.com/odata/{objectName}");

            // Parse response
            var content = Deserialize<IEnumerable<T>>(response);

            return content;
        }

        private T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = new TCNamingPolicy()
            });
        }
    }
}
