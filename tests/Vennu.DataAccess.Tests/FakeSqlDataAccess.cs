using Vennu.DataAccess;
using System.Linq.Expressions;

namespace Vennu.DataAccess.Tests;

internal sealed class FakeSqlDataAccess : ISqlDataAccess
{
    public Func<object, object?>? QueryHandler { get; set; }

    public Func<object, IEnumerable<object>>? QueryManyHandler { get; set; }

    public Func<Type, IEnumerable<object>>? QueryAllHandler { get; set; }

    public Func<string, object, IEnumerable<object>>? ExecuteSqlQueryHandler { get; set; }

    public CancellationToken? LastCancellationToken { get; private set; }

    public List<object> InsertedEntities { get; } = [];

    public List<object> UpdatedEntities { get; } = [];

    public int InsertResult { get; set; } = 1;

    public int UpdateResult { get; set; } = 1;

    public int Insert<T>(T entity) where T : class
    {
        InsertedEntities.Add(entity);
        return InsertResult;
    }

    public int InsertAll<T>(IEnumerable<T> entities) where T : class
    {
        foreach (var entity in entities)
        {
            InsertedEntities.Add(entity!);
        }

        return InsertResult;
    }

    public Task<int> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(Insert(entity));
    }

    public Task<int> InsertAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(InsertAll(entities));
    }

    public T? Query<T>(object criteria) where T : class => QueryHandler?.Invoke(criteria) as T;

    public Task<T?> QueryAsync<T>(object criteria, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(Query<T>(criteria));
    }

    public IEnumerable<T> Query<T, TParameters>(string tableName, TParameters parameters) where T : class =>
        Query<T, TParameters>(parameters);

    public Task<IEnumerable<T>> QueryAsync<T, TParameters>(string tableName, TParameters parameters, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(Query<T, TParameters>(tableName, parameters));
    }

    public IEnumerable<T> Query<T, TParameters>(TParameters parameters) where T : class =>
        (QueryManyHandler?.Invoke(parameters!) ?? []).Cast<T>();

    public Task<IEnumerable<T>> QueryAsync<T, TParameters>(TParameters parameters, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(Query<T, TParameters>(parameters));
    }

    public IEnumerable<T> QueryAll<T>() where T : class =>
        (QueryAllHandler?.Invoke(typeof(T)) ?? []).Cast<T>();

    public Task<IEnumerable<T>> QueryAllAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(QueryAll<T>());
    }

    public Task<IEnumerable<T>> ExecuteSqlQueryAsync<T, TParameters>(
        string sql,
        TParameters parameters,
        CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult((ExecuteSqlQueryHandler?.Invoke(sql, parameters!) ?? []).Cast<T>());
    }

    public int Update<T>(T entity) where T : class
    {
        UpdatedEntities.Add(entity);
        return UpdateResult;
    }

    public int UpdateAll<T>(IEnumerable<T> entities) where T : class
    {
        foreach (var entity in entities)
        {
            UpdatedEntities.Add(entity!);
        }

        return UpdateResult;
    }

    public int UpdateAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class =>
        UpdateAll(entities);

    public Task<int> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(Update(entity));
    }

    public Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(UpdateAll(entities));
    }

    public Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(UpdateAll(entities, keyCheck));
    }

    public int MergeAll<T>(IEnumerable<T> entities) where T : class =>
        UpdateAll(entities);

    public int MergeAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class =>
        UpdateAll(entities, keyCheck);

    public int MergeAll<T>(IEnumerable<T> entities, string tableName) where T : class =>
        UpdateAll(entities);

    public Task<int> MergeAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(MergeAll(entities));
    }

    public Task<int> MergeAllAsync<T>(IEnumerable<T> entities, string tableName, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(MergeAll(entities, tableName));
    }

    public int DeleteAll<T>(string[] keys) where T : class =>
        UpdateResult;

    public int DeleteAll<T>(string tableName) where T : class =>
        UpdateResult;

    public Task<int> DeleteAllAsync<T>(string[] keys, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(DeleteAll<T>(keys));
    }

    public Task<int> DeleteAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(DeleteAll<T>(tableName));
    }
}
