﻿using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TCSlackbot.Logic.Authentication.Exceptions;
using TCSlackbot.Logic.Resources;

namespace TCSlackbot.Logic.Utils
{
    public class TokenManager : ITokenManager
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ISecretManager _secretManager;

        public TokenManager(HttpClient client, IConfiguration configuration, ISecretManager secretManager)
        {
            _client = client;
            _configuration = configuration;
            _secretManager = secretManager;
        }

        /// <summary>
        /// Finds the access token for the specified user in the cache or via the refresh token by renewing it.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>Either the access token if successful or default if something went wrong 
        /// (The user may need to login again if the refresh token in the key vault was invalid.)</returns>
        public async Task<string?> GetAccessTokenAsync(string userId)
        {
            try
            {
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
                var (accessToken, refreshToken) = await RenewTokensAsync(oldRefreshToken);

                // Tokens could not be renewed
                if (accessToken is null || refreshToken is null)
                {
                    // Delete the refresh token if it's invalid
                    await _secretManager.DeleteSecretAsync(userId);

                    throw new LoggedOutException();
                }

                //
                // Update the refresh token in the keyvault
                //
                _secretManager.SetSecret(userId, refreshToken);

                return accessToken;
            }
            catch (LoggedOutException)
            {
                // Throw new exception, so you don't have to rethrow the catched exception (this would change the stack information)
                // 
                throw new LoggedOutException();
            }
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
            var discoveryResponse = await _client.GetDiscoveryDocumentAsync("https://auth.timecockpit.com/");

            //
            // Send request to the auth endpoint
            //
            using var rtRequest = new RefreshTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _configuration["TimeCockpit-ClientId"],
                ClientSecret = _configuration["TimeCockpit-ClientSecret"],
                Scope = "openid offline_access",
                RefreshToken = oldRefreshToken,
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader
            };
            var response = await _client.RequestRefreshTokenAsync(rtRequest);

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
