using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ICosmosManager
    {
        public Task<T> GetDocumentAsync<T>(string collectionName, string documentId);

        public Task<T> CreateDocumentAsync<T>(string collectionName, T document);

        public SlackUser GetSlackUser(string collectionName, string userId);
    }
}
