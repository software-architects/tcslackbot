using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ICosmosManager
    {
        public Task<T> GetDocumentAsync<T>(string collectionName, string documentId);

        public Task<ResourceResponse<Document>> CreateDocumentAsync<T>(string collectionName, T document);
    }
}
