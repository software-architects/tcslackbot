using System.Collections.Generic;
using System.Threading.Tasks;
using TCSlackbot.Logic.TimeCockpit;
using TCSlackbot.Logic.TimeCockpit.Objects;

namespace TCSlackbot.Logic.Utils
{
    public interface ITCManager
    {
        /// <summary>
        /// Requests data from the server. 
        /// </summary>
        /// <typeparam name="T">The type of the object which will be retrieved (you don't have to add 'APP_' yourself)</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <returns>The list of objects</returns>
        public Task<IEnumerable<T>> GetObjectsAsync<T>(string accessToken);

        /// <summary>
        /// Requests filtered data from the server.
        /// </summary>
        /// <typeparam name="T">The type of the object which will be retrieved (you don't have to add 'APP_' yourself)</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="queryData">The query data (contains all the options needed for the request)</param>
        /// <returns>The list of objects</returns>
        public Task<IEnumerable<T>> GetFilteredObjectsAsync<T>(string accessToken, TCQueryData queryData);

        /// <summary>
        /// Wrapper that returns all projects.
        /// </summary>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="projectFilter">The name of the filter</param>
        /// <returns>The list of filtered objects</returns>
        public Task<IEnumerable<Project>> GetFilteredProjects(string accessToken, string projectFilter);
    }
}
