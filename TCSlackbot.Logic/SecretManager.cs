using Microsoft.Extensions.Configuration;

namespace TCSlackbot.Logic
{
    public class SecretManager : ISecretManager
    {
        private IConfiguration _configuration;

        public SecretManager(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string GetSecret(string key)
        {
            return _configuration[key];
        }
    }
}
