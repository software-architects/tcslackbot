namespace TCSlackbot.Logic
{
    public interface ISecretManager
    {
        public string GetSecret(string key);
    }
}
