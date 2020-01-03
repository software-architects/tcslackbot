using System.Threading.Tasks;

namespace TCSlackbot.Logic
{
    public interface ISecretManager
    {
        string GetSecret(string key);
        void SetSecret(string key, string value);
        Task DeleteSecretAsync(string key);
    }
}
