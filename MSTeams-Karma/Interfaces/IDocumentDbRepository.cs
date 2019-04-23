using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace MSTeams.Karma
{
    public interface IDocumentDbRepository<T> where T : class
    {
        Task<T> GetItemAsync(string id, string partition, CancellationToken cancellationToken);
        Uri GetCollectionUri();
        Task<Document> CreateItemAsync(T item, string partition, CancellationToken cancellationToken, bool disableAutomaticIdGeneration = true);
        Task<Document> UpdateItemAsync(string id, T item, string partition, CancellationToken cancellationToken);
        Task<T> ExecuteStoredProcedureAsync(string storedProcedureLink, string partition, CancellationToken cancellationToken, params dynamic[] procedureParams);
        void Initialize();
        DocumentClient GetDocumentClient(string endpoint = null, string authKey = null);
    }
}