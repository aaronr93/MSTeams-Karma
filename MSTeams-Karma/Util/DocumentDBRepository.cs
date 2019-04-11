using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace MSTeams.Karma
{
    public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : class
    {
        public DocumentDbRepository(string collectionId)
        {
            _collectionId = collectionId;
            Initialize();
        }

        private readonly string _collectionId;
        private readonly string _databaseId = ConfigurationManager.AppSettings["database"];
        private readonly string _authKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
        private readonly string _endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];

        private RequestOptions RequestOptions(object partitionValue) => new RequestOptions
        { 
            PartitionKey = new PartitionKey(partitionValue)
        };

        private DocumentClient _client;

        public async Task<T> GetItemAsync(string id, string partition)
        {
            try
            {
                return await _client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), RequestOptions(partition));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item, string partition, bool disableAutomaticIdGeneration = true)
        {
            return await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), item, RequestOptions(partition), disableAutomaticIdGeneration);
        }

        public async Task<Document> UpdateItemAsync(string id, T item, string partition)
        {
            return await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), item, RequestOptions(partition));
        }

        public async Task DeleteItemAsync(string id, string partition)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), RequestOptions(partition));
        }

        public void Initialize()
        {
            _client = GetDocumentClient(_endpoint, _authKey);
        }

        public DocumentClient GetDocumentClient(string endpoint, string authKey)
        {
            return new DocumentClient(new Uri(endpoint), authKey);
        }
    }
}