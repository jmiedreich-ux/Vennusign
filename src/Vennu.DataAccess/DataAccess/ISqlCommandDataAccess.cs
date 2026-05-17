namespace Vennu.DataAccess;

using RepoDb;
using System.Linq.Expressions;

public interface ISqlCommandDataAccess
{
    int Insert<T>(T entity) where T : class;

    int InsertAll<T>(IEnumerable<T> entities) where T : class;

    int Update<T>(T entity) where T : class;

    int UpdateAll<T>(IEnumerable<T> entities) where T : class;

    int UpdateAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class;

    int MergeAll<T>(IEnumerable<T> entities) where T : class;

    int MergeAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class;

    int MergeAll<T>(IEnumerable<T> entities, string tableName) where T : class;

    int DeleteAll<T>(string[] keys) where T : class;

    int DeleteAll<T>(string tableName) where T : class;

    Task<int> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task<int> InsertAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;

    Task<int> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;

    Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;

    Task<int> MergeAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;

    Task<int> MergeAllAsync<T>(IEnumerable<T> entities, string tableName, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;

    Task<int> DeleteAllAsync<T>(string[] keys, CancellationToken cancellationToken = default) where T : class;

    Task<int> DeleteAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class;
}
