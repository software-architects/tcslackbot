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
        public async Task<T> CreateObjectAsync<T>(string accessToken, T data)
        {
            // Get the object name
            var objectName = $"APP_{typeof(T).Name}";

            // Send request
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.timecockpit.com/odata/{objectName}"),
            };

            var jsonData = JsonSerializer.Serialize(data);
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse response
            var content = Serializer.Deserialize<T>(responseContent);

            return content;
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

            return content?.Value == null ? new List<T>() : (IEnumerable<T>)content.Value.ToArray();
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

            return content?.Value == null ? new List<T>() : (IEnumerable<T>)content.Value.ToArray();
        }

        /// <inheritdoc/>
        public async Task<T?> GetObjectAsync<T>(string accessToken, TCQueryData queryData) where T : class
        {
            var objects = await GetFilteredObjectsAsync<T>(accessToken, queryData);

            if (objects.Count() == 1)
            {
                return objects.FirstOrDefault();
            }
            else
            {
                return default;
            }
        }


        /// <inheritdoc/>
        public async Task<IEnumerable<Project>> GetFilteredProjects(string accessToken, string projectName)
        {
            var queryData = new TCQueryData($"From P In Project Where P.Code Like '%{projectName}%' Select P");
            return await GetFilteredObjectsAsync<Project>(accessToken, queryData);
        }

        /// <inheritdoc/>
        public async Task<Project?> GetProjectAsync(string accessToken, string projectName)
        {
            var queryData = new TCQueryData($"From P In Project Where P.Code Like '%{projectName}%' Select P");
            return await GetObjectAsync<Project>(accessToken, queryData);
        }

        /// <inheritdoc/>
        public async Task<UserDetail?> GetCurrentUserDetailsAsync(string accessToken)
        {
            var queryData = new TCQueryData("From U In UserDetail Where U.UserDetailUuid = Environment.CurrentUser.APP_UserDetailUuid Select U");

            return await GetObjectAsync<UserDetail>(accessToken, queryData);
        }

    }
}
