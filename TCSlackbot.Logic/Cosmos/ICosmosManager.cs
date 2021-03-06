﻿using Microsoft.Azure.Documents.Linq;
using System.Threading.Tasks;

namespace TCSlackbot.Logic.Utils
{
    public interface ICosmosManager
    {
        /// <summary>
        /// Retrieves a document from the database.
        /// </summary>
        /// <typeparam name="T">The type of the stored document</typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <param name="documentId">The id of the document</param>
        /// <returns>Default if it failed or the document</returns>
        public Task<T?> GetDocumentAsync<T>(string collectionName, string documentId) where T : class;

        /// <summary>
        /// Get all SlackUsers in the DB which are working longer thän a specific amount of time.
        /// </summary>
        /// <returns></returns>
        public IDocumentQuery<SlackUser>? GetAllSlackUsers();

        /// <summary>
        /// Creates a new document in the database.
        /// </summary>
        /// <typeparam name="T">The type of the document</typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <param name="document">The document to create</param>
        /// <returns>Default if it failed or the created document</returns>
        public Task<T?> CreateDocumentAsync<T>(string collectionName, T document) where T : class;

        /// <summary>
        /// Replaces the specified document in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <param name="document">The document to create</param>
        /// <param name="documentId">The id of the document</param>
        /// <returns>Default if it failed or the replaced document</returns>
        public Task<T?> ReplaceDocumentAsync<T>(string collectionName, T document, string documentId) where T : class;

        public Task RemoveDocumentAsync(string collectionName, string documentId);

        public bool ExistsDocument(string collectionName, string documentId);
    }
}
