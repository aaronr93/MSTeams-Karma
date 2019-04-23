using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace MSTeams.Karma
{
    public interface IDocumentDbRepository<T> where T : class
    {
        Task<T> GetItemAsync(string id, string partition, CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
        Task<Document> CreateItemAsync(T item, string partition, CancellationToken cancellationToken, bool disableAutomaticIdGeneration = true);
        Task<Document> UpdateItemAsync(string id, T item, string partition, CancellationToken cancellationToken);
        Task DeleteItemAsync(string id, string partition, CancellationToken cancellationToken);
        void Initialize();
        DocumentClient GetDocumentClient(string endpoint, string authKey);
    }
}