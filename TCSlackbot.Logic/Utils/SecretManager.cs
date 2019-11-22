using Microsoft.Extensions.Configuration;

namespace TCSlackbot.Logic
{
    public class SecretManager : ISecretManager
    {
        private readonly IConfiguration _configuration;

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
    }
}
