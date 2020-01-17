using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ITokenManager
    {
        /// <summary>
        /// Retrieves the access token either from the cache or requests a new one.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The access token or default if it could not be renewed</returns>
        public Task<string> GetAccessTokenAsync(string userId);

        /// <summary>
        /// Renews the access and refresh token via the refresh token. 
        /// </summary>
        /// <param name="oldRefreshToken">The old refresh token which should be used for the renewal</param>
        /// <returns>The access token and refresh token or default if it went wrong</returns>
        public Task<(string, string)> RenewTokensAsync(string oldRefreshToken);
    }
}
