namespace TCSlackbot.Logic
{
    public interface ISecretManager
    {
        string GetSecret(string key);
        void SetSecret(string key, string value);
    }
}
