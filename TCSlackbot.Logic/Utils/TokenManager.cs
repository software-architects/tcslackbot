using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public class TokenManager : ITokenManager
    {
        private static readonly HttpClient client = new HttpClient();
        public static readonly AccessTokenCache accessTokenCache = new AccessTokenCache();

        private readonly IConfiguration _configuration;
        private readonly ISecretManager _secretManager;

        public TokenManager(IConfiguration configuration, ISecretManager secretManager)
        {
            _configuration = configuration;
            _secretManager = secretManager;
        }

        /// <summary>
        /// Finds the access token for the specified user in the cache or via the refresh token by renewing it.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>Either the access token if successful or default if something went wrong 
        /// (The user may need to login again if the refresh token in the key vault was invalid.)</returns>
        public async Task<string> GetAccessTokenAsync(string userId)
        {
            //
            // Check if already in the cache
            //
            if (accessTokenCache.HasValidToken(userId))
            {
                return accessTokenCache.Get(userId);
            }

            //
            // Get the refresh token
            //
            var oldRefreshToken = _secretManager.GetSecret(userId);

            // No refresh token found -> User needs to login first
            if (oldRefreshToken is null)
            {
                return default;
            }

            //
            // Get the new access and refresh token
            //
            var (accessToken, refreshToken) = await RenewTokensAsync(userId);

            // Tokens could not be renewed
            if (accessToken is null || refreshToken is null)
            {
                // Delete the refresh token if it's invalid
                await _secretManager.DeleteSecretAsync(userId);

                return default;
            }

            //
            // Update the refresh token in the keyvault
            //
            _secretManager.SetSecret(userId, refreshToken);

            //
            // Add it to the cache
            //
            accessTokenCache.Add(userId, accessToken);

            return accessToken;
        }

        /// <summary>
        /// Renews the access and refresh token. 
        /// </summary>
        /// <param name="oldRefreshToken">The old refresh token which will be used to generate a new access and refresh token.</param>
        /// <returns>A tuple with the access token (0) and refresh token (1)</returns>
        public async Task<(string, string)> RenewTokensAsync(string oldRefreshToken)
        {
            //
            // Find the discovery endpoint
            //
            var discoveryResponse = await client.GetDiscoveryDocumentAsync("https://auth.timecockpit.com/");

            //
            // Send request to the auth endpoint
            //
            var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _configuration["TimeCockpit-ClientId"],
                ClientSecret = _configuration["TimeCockpit-ClientSecret"],
                Scope = "openid offline_access",
                RefreshToken = oldRefreshToken,
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader
            });

            // Could not renew the tokens
            if (response.IsError)
            {
                return default;
            }

            //
            // Get the new access and refresh token
            //
            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;

            return (accessToken, refreshToken);
        }
    }
}
