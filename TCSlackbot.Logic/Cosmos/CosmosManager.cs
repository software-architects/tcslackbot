using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TCSlackbot.Logic.Cosmos;

namespace TCSlackbot.Logic.Utils
{
    public class CosmosManager : ICosmosManager
    {
        private const string DatabaseName = "tcslackbot";

        private static DocumentClient? client;

        private readonly IConfiguration _configuration;
        private readonly ILogger<CosmosManager> _logger;

        public CosmosManager(ILogger<CosmosManager> logger, IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new InvalidProgramException();
            }

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

        /// <inheritdoc/>
        public async Task<T?> GetDocumentAsync<T>(string collectionName, string documentId) where T: class
        {
            if (client is null)
            {
                return default;
            }

            try
            {
                var documentUri = UriFactory.CreateDocumentUri(DatabaseName, collectionName, documentId);
                var response = await client.ReadDocumentAsync(documentUri);

                return (T)(dynamic)response.Resource;
            }
            catch (DocumentClientException)
            {
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public IDocumentQuery<SlackUser>? GetAllSlackUsers()
        {
            if (client is null)
            {
                return default;
            }

            try
            {
                var response = client.CreateDocumentQuery<SlackUser>(
                    Collection.Users,
                    new FeedOptions { MaxItemCount = 10 })
                    .Where(s => s.IsWorking && s.Worktime != null && s.Worktime.Start != null && (DateTime.Now - s.Worktime.Start).Value > TimeSpan.FromHours(4))
                    .AsDocumentQuery();

                return (dynamic)response;
            }
            catch (DocumentClientException)
            {
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T?> CreateDocumentAsync<T>(string collectionName, T document) where T: class
        {
            if (client is null)
            {
                return default;
            }

            await CreateCollectionIfNotExistsAsync(DatabaseName, collectionName);

            try
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                var response = await client.CreateDocumentAsync(collectionUri, document);

                return (T)(dynamic)response.Resource;
            }
            catch (DocumentClientException)
            {
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T?> ReplaceDocumentAsync<T>(string collectionName, T document, string documentId) where T: class
        {
            if (client is null)
            {
                return default;
            }

            try
            {
                var documentUri = UriFactory.CreateDocumentUri(DatabaseName, collectionName, documentId);
                var response = await client.ReplaceDocumentAsync(documentUri, document);

                return (T)(dynamic)response.Resource;
            }
            catch (DocumentClientException)
            {
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveDocumentAsync(string collectionName, string documentId)
        {
            if (client is null)
            {
                return;
            }

            try
            {
                var documentUri = UriFactory.CreateDocumentUri(DatabaseName, collectionName, documentId);
                await client.DeleteDocumentAsync(documentUri);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ExistsDocument(string collectionName, string documentId)
        {
            if (client is null)
            {
                return default;
            }

            try
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                var query = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });

                return query.Any(x => x.Id == documentId);
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Creates the specified database if it does not yet exist.
        /// </summary>
        /// <param name="databaseId">The name of the database</param>
        /// <returns></returns>
        private static async Task CreateDatabaseIfNotExistsAsync(string databaseId)
        {
            if (client is null)
            {
                return;
            }

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

        /// <summary>
        /// Creates the specified collection if it does not yet exist.
        /// </summary>
        /// <param name="databaseId">The name of the database</param>
        /// <param name="collectionId">The name of the collection</param>
        /// <returns></returns>
        private static async Task CreateCollectionIfNotExistsAsync(string databaseId, string collectionId)
        {
            if (client is null)
            {
                return;
            }

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
