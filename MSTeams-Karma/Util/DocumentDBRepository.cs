using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

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

        public async Task<T> GetItemAsync(string id, string partition, CancellationToken cancellationToken)
        {
            try
            {
                return await _client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), RequestOptions(partition), cancellationToken);
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

        public Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }

        public async Task<Document> CreateItemAsync(T item, string partition, CancellationToken cancellationToken, bool disableAutomaticIdGeneration = true)
        {
            return await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), item, RequestOptions(partition), disableAutomaticIdGeneration, cancellationToken);
        }

        public async Task<Document> UpdateItemAsync(string id, T item, string partition, CancellationToken cancellationToken)
        {
            return await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), item, RequestOptions(partition), cancellationToken);
        }

        public async Task<T> ExecuteStoredProcedureAsync(string storedProcedureLink, string partition, CancellationToken cancellationToken, params dynamic[] procedureParams)
        {
            var rawResult = await _client.ExecuteStoredProcedureAsync<string>(storedProcedureLink, RequestOptions(partition), cancellationToken, procedureParams);
            return JsonConvert.DeserializeObject<T>(rawResult.Response);
        }

        public void Initialize()
        {
            _client = GetDocumentClient(_endpoint, _authKey);
        }

        public DocumentClient GetDocumentClient(string endpoint = null, string authKey = null)
        {
            return new DocumentClient(new Uri(endpoint ?? _endpoint), authKey ?? _authKey);
        }
    }
}