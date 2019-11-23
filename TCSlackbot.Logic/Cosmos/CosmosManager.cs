using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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
                client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName }).Wait();
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

        public async Task<T> GetDocumentAsync<T>(string collectionName, string documentId)
        {
            var uri = UriFactory.CreateDocumentUri(DatabaseName, collectionName, documentId);
            var response = await client.ReadDocumentAsync(uri);

            return (T)(dynamic)response.Resource;
        }

        public async Task<ResourceResponse<Document>> CreateDocumentAsync<T>(string collectionName, T document)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
            var response = await client.CreateDocumentAsync(collectionUri, document);

            return response;
        }
    }
}
