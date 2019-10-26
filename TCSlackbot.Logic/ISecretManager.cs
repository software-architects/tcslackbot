namespace TCSlackbot.Logic
{
    public interface ISecretManager
    {
        string GetSecret(string key);
    }
}
