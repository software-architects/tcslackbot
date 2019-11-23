using Microsoft.Azure.Documents;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ICosmosManager
    {
        public Task<Document> GetDocumentAsync<T>(string collectionName, string documentId);

        public Task<Document> CreateDocumentAsync<T>(string collectionName, T document);

        public Task<Document> ReplaceDocumentAsync(Document document);


        public SlackUser GetSlackUser(string collectionName, string userId);

        public Task<SlackUser> ReplaceSlackUserAsync(string collectionName, SlackUser slackUser);
    }
}
