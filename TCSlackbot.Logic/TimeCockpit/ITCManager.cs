using System.Collections.Generic;
using System.Threading.Tasks;
using TCSlackbot.Logic.TimeCockpit;
using TCSlackbot.Logic.TimeCockpit.Objects;

namespace TCSlackbot.Logic.Utils
{
    public interface ITCManager
    {
        /// <summary>
        /// Creates the specified object.
        /// </summary>
        /// <typeparam name="T">The type of the object that will be created. The type will be used for the url: 'APP_TYPE'</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="data">The object which should be created</param>
        /// <returns>The created object</returns>
        public Task<T?> CreateObjectAsync<T>(string accessToken, T data) where T : class;

        /// <summary>
        /// Requests data from the server. 
        /// </summary>
        /// <typeparam name="T">The type of the object which will be retrieved (you don't have to add 'APP_' yourself)</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <returns>The list of objects</returns>
        public Task<IEnumerable<T>?> GetObjectsAsync<T>(string accessToken);

        /// <summary>
        /// Requests filtered data from the server.
        /// </summary>
        /// <typeparam name="T">The type of the object which will be retrieved (you don't have to add 'APP_' yourself)</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="queryData">The query data (contains all the options needed for the request)</param>
        /// <returns>The list of objects</returns>
        public Task<IEnumerable<T>?> GetFilteredObjectsAsync<T>(string accessToken, TCQueryData queryData);

        /// <summary>
        /// Returns the object of the query (calls internally `GetFilteredObjectsAsync`, and checks if it only returns one element).
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="queryData">The query data (contains all the options needed for the request)</param>
        /// <returns>The object or default</returns>
        public Task<T?> GetObjectAsync<T>(string accessToken, TCQueryData queryData) where T : class;

        /// <summary>
        /// Wrapper that returns all projects.
        /// </summary>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="projectFilter">The name of the filter</param>
        /// <returns>The list of filtered objects</returns>
        public Task<IEnumerable<Project>> GetFilteredProjects(string accessToken, string projectFilter);

        /// <summary>
        /// Requests the object and returns it.
        /// </summary>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <param name="projectName">The name of the specified object</param>
        /// <returns>The object if only one found or default</returns>
        public Task<Project?> GetProjectAsync(string accessToken, string projectName);

        /// <summary>
        /// Retrieves the user details (uuid, ...) of the currently logged in user.
        /// </summary>
        /// <param name="accessToken">The access token for the specified user</param>
        /// <returns>The user details or default</returns>
        public Task<UserDetail?> GetCurrentUserDetailsAsync(string accessToken);
    }
}
