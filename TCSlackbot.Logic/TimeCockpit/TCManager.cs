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

        /// <summary>
        /// Sends a http request to the with data to the time cockpit api.
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="method">The http request method</param>
        /// <param name="accessToken">The access token of the user</param>
        /// <param name="uri">The uri of the request</param>
        /// <param name="jsonData">The serialized data which can be sent</param>
        /// <returns>The object or null</returns>
        private async Task<T?> SendPostRequest<T>(HttpMethod method, string accessToken, string uri, string? jsonData) where T : class
        {
            using var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(uri),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (jsonData != null)
            {
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            var response = await _client.SendAsync(request);

            // Invalid content type: 'text/html; charset=utf-8'
            //  -> This is for example the login page, if the access token is invalid.
            // Valid content type: 'application/json; charset=utf-8'
            var contentType = response.Content.Headers.ContentType.ToString();

            // Validate the response
            if (response.IsSuccessStatusCode && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Serializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                Console.WriteLine(response);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<T?> CreateObjectAsync<T>(string accessToken, T data) where T : class
        {
            var objectName = $"APP_{typeof(T).Name}";
            return await SendPostRequest<T>(HttpMethod.Post, accessToken, $"https://web.timecockpit.com/odata/{objectName}", JsonSerializer.Serialize(data));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>?> GetObjectsAsync<T>(string accessToken)
        {
            var objectName = $"APP_{typeof(T).Name}";
            var response = await SendPostRequest<ODataResponse<T>>(HttpMethod.Get, accessToken, $"https://web.timecockpit.com/odata/{objectName}", null);
            
            return response?.Value == null ? new List<T>() : (IEnumerable<T>)response.Value.ToArray();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>?> GetFilteredObjectsAsync<T>(string accessToken, TCQueryData queryData)
        {
            var response = await SendPostRequest<ODataResponse<T>>(HttpMethod.Post, accessToken, "https://web.timecockpit.com/select", JsonSerializer.Serialize(queryData));

            return response?.Value == null ? new List<T>() : (IEnumerable<T>)response.Value.ToArray();
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
            var projects = await GetFilteredObjectsAsync<Project>(accessToken, queryData);

            return projects is null ? new List<Project>() : projects;
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
