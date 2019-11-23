using Microsoft.Azure.Documents;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ICosmosManager
    {
        public Task<Document> GetDocumentAsync(string collectionName, string documentId);

        public Task<Document> CreateDocumentAsync<T>(string collectionName, T document);

        public Task<Document> ReplaceDocumentAsync(Document document);


        public SlackUser GetSlackUser(string collectionName, string userId);
    }
}
