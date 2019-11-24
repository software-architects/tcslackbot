using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public class CosmosManager : ICosmosManager
    {
        private const string DatabaseName = "tcslackbot";

        private static DocumentClient client;

        private readonly IConfiguration _configuration;
        private readonly ILogger<CosmosManager> _logger;

        public CosmosManager(ILogger<CosmosManager> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;

            try
            {
                var uri = _configuration["Cosmos-URI"];
                var key = _configuration["Cosmos-PrimaryKey"];

                // Create the client
                client = new DocumentClient(new Uri(uri), key);

                // Create database if it does not exist yet
                CreateDatabaseIfNotExistsAsync(DatabaseName).Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                _logger.LogCritical("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                _logger.LogCritical("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }


        public async Task<Document> GetDocumentAsync(string collectionName, string documentId)
        {
            var uri = UriFactory.CreateDocumentUri(DatabaseName, collectionName, documentId);
            var response = await client.ReadDocumentAsync(uri);

            return response.Resource;
        }

        public async Task<Document> CreateDocumentAsync<T>(string collectionName, T document)
        {
            await CreateCollectionIfNotExistsAsync(DatabaseName, collectionName);

            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
            var response = await client.CreateDocumentAsync(collectionUri, document);

            return response.Resource;
        }

        public async Task<Document> ReplaceDocumentAsync(Document document)
        {
            var response = await client.ReplaceDocumentAsync(document);

            return response.Resource;
        }


        public SlackUser GetSlackUser(string collectionName, string userId)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
            try
            {
                var result = client.CreateDocumentQuery<SlackUser>(collectionUri)
                    .Where(user => user.UserId == userId).ToList();
                // 
                // More than one user with the same id found -> Should never happen
                // 
                if (result.Count() > 1)
                {
                    return default;
                }

                return result.FirstOrDefault();
            }
            catch (AggregateException ex)
            {
                //Ignore it
            }
           
            
            return null;
            
        }


        private static async Task CreateDatabaseIfNotExistsAsync(string databaseId)
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync(string databaseId, string collectionId)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
