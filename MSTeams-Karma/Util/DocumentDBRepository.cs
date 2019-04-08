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
    public interface IDocumentDbRepository<T> where T : class
    {
        Task<T> GetItemAsync(string id, string partition);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);
        Task<Document> CreateItemAsync(T item, string partition);
        Task<Document> UpdateItemAsync(string id, T item, string partition);
        Task DeleteItemAsync(string id, string partition);
        void Initialize();
        DocumentClient GetDocumentClient(string endpoint, string authKey);
    }

    public class DocumentDBRepository<T> : IDocumentDbRepository<T>
        where T : class
    {
        public static IDocumentDbRepository<T> Default = new DocumentDBRepository<T>();

        private readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private readonly string AuthKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
        private readonly string Endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];

        private RequestOptions RequestOptions(object partitionValue) => new RequestOptions
        { 
            PartitionKey = new PartitionKey(partitionValue)
        };

        private DocumentClient client;

        public async Task<T> GetItemAsync(string id, string partition)
        {
            try
            {
                return await client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), RequestOptions(partition));
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
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
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

        public async Task<Document> CreateItemAsync(T item, string partition)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item, RequestOptions(partition), disableAutomaticIdGeneration: true);
        }

        public async Task<Document> UpdateItemAsync(string id, T item, string partition)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item, RequestOptions(partition));
        }

        public async Task DeleteItemAsync(string id, string partition)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), RequestOptions(partition));
        }

        public void Initialize()
        {
            client = GetDocumentClient(Endpoint, AuthKey);
        }

        public DocumentClient GetDocumentClient(string endpoint, string authKey)
        {
            return new DocumentClient(new Uri(endpoint), authKey);
        }
    }
}