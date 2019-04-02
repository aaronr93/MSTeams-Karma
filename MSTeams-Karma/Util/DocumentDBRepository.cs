namespace MSTeams.Karma
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public static class DocumentDBRepository<T> where T : class
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static readonly string AuthKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
        private static readonly string Endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];
        public const string DefaultPartition = "generalpartition";

        private static RequestOptions RequestOptions(object partitionValue) => new RequestOptions
        { 
            PartitionKey = new PartitionKey(partitionValue)
        };

        private static DocumentClient client;

        public static async Task<T> GetItemAsync(string id, string partition = DefaultPartition)
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

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
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

        public static async Task<Document> CreateItemAsync(T item, string partition = DefaultPartition)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item, RequestOptions(partition), disableAutomaticIdGeneration: true);
        }

        public static async Task<Document> UpdateItemAsync(string id, T item, string partition = DefaultPartition)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item, RequestOptions(partition));
        }

        public static async Task DeleteItemAsync(string id, string partition = DefaultPartition)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), RequestOptions(partition));
        }

        public static void Initialize()
        {
            client = GetDocumentClient(Endpoint, AuthKey);
        }

        public static DocumentClient GetDocumentClient(string endpoint, string authKey)
        {
            return new DocumentClient(new Uri(endpoint), authKey);
        }
    }
}