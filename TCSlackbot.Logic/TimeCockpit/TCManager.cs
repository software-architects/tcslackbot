using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCSlackbot.Logic.TimeCockpit;
using TCSlackbot.Logic.TimeCockpit.Objects;

namespace TCSlackbot.Logic.Utils
{
    public class TCManager : ITCManager
    {
        private readonly HttpClient _client;

        public TCManager(HttpClient client)
        {
            _client = client;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetObjectsAsync<T>(string accessToken)
        {
            // Get the object name
            var objectName = $"APP_{typeof(T).Name}";

            // Send request
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://web.timecockpit.com/odata/{objectName}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse response
            var content = Serializer.Deserialize<ODataResponse<T>>(responseContent);

            return content.Value.ToArray();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetFilteredObjectsAsync<T>(string accessToken, TCQueryData queryData)
        {
            // Send request
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(queryData), Encoding.UTF8, "application/json"),
                RequestUri = new Uri($"https://web.timecockpit.com/select"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse response
            var content = Serializer.Deserialize<ODataResponse<T>>(responseContent);

            return content.Value.ToArray();
        }
    }
}
