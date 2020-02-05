using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace TCSlackbot.Logic
{
    public class AccessTokenCache
    {
        private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        /// <summary>
        /// Checks, whether there's an access token that it still valid.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>True if there's an valid access token</returns>
        public bool HasValidToken(string userId)
        {
            if (!_dictionary.ContainsKey(userId))
            {
                return false;
            }

            return Get(userId) != default;
        }

        /// <summary>
        /// Adds a new access token to the cache.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="token">The access token</param>
        /// <returns>False if the token was not valid</returns>
        public bool Add(string userId, string token)
        {
            if (!IsValidToken(token))
            {
                return false;
            }

            _dictionary.Add(userId, token);

            return true;
        }

        /// <summary>
        /// Returns a valid access token for the specified user.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The access token or default if it is already expired</returns>
        public string Get(string userId)
        {
            //
            // Get the token
            // 
            var token = _dictionary.GetValueOrDefault(userId);
            if (token == default)
            {
                return default;
            }

            //
            // Delete the token if it's not valid
            //
            if (!IsValidToken(token))
            {
                _dictionary.Remove(userId);
                return default;
            }

            return token;
        }

        /// <summary>
        /// Checks, whether the token has expired.
        /// </summary>
        /// <param name="token">The specified token to check</param>
        /// <returns>True if it's has not yet expired</returns>
        private static bool IsValidToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwtToken == null)
            {
                return false;
            }

            return jwtToken.ValidTo > DateTime.UtcNow;
        }
    }
}
