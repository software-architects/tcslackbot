using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace TCSlackbot.Logic
{
    public class SecretManager : ISecretManager
    {
        private readonly IConfiguration _configuration;
        private readonly string KeyVaultEndpoint = "https://tcslackbot-key-vault.vault.azure.net/";

        public SecretManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetSecret(string key)
        {
            return _configuration[key];
        }

        public void SetSecret(string key, string value)
        {
            _configuration[key] = value;
        }

        public async Task DeleteSecretAsync(string key)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            if (_configuration[key] == null)
            {
                return;
            }

            // Delete the secret in the key vault
            //
            try
            {
                await keyVaultClient.DeleteSecretAsync(KeyVaultEndpoint, key);
            }
            catch (KeyVaultErrorException ex)
            {
                Console.WriteLine("\n\nError:" + ex.Message);
            }

            // Delete it locally (needed in case the secret cannot be deleted). If the above statement fails, this will save it. 
            // Without this, the user may have an invalid refresh token stored in the key vault and can't use the bot anymore.
            // 
            _configuration[key] = null;

            // Reload the configuration because we added a new secret
            ((IConfigurationRoot)_configuration).Reload();
        }
    }
}
