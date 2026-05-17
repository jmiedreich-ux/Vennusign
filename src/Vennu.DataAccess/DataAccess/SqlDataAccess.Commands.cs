using System.Dynamic;
using System.Linq.Expressions;
using RepoDb;

namespace Vennu.DataAccess;

public partial class SqlDataAccess
{
    public int Update(string tableName, Dictionary<string, object> recordData) =>
        Execute(nameof(Update), connection => DbConnectionExtension.Update(connection, tableName, entity: recordData), new { tableName, recordData });

    public int Update<T>(T entity, IEnumerable<Field> fields) where T : class =>
        Execute(nameof(Update), connection => DbConnectionExtension.Update(connection, entity: entity, fields: fields), new { entity, fields });

    public int Update<T>(T entity) where T : class =>
        Execute(nameof(Update), connection => DbConnectionExtension.Update(connection, entity), entity);

    public Task<int> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync(
            nameof(UpdateAsync),
            (connection, token) => DbConnectionExtension.UpdateAsync(connection, entity, cancellationToken: token),
            entity,
            cancellationToken);

    public int Update<T>(T entity, Expression<Func<T, bool>> keyCheck) where T : class =>
        Execute(nameof(Update), connection => DbConnectionExtension.Update(connection, entity, keyCheck), new { entity, keyCheck });

    public int UpdateAll<T>(IEnumerable<T> entities) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(UpdateAll), connection => DbConnectionExtension.UpdateAll(connection, entityList), entityList);
    }

    public Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToList();
        return ExecuteAsync(
            nameof(UpdateAllAsync),
            (connection, token) => DbConnectionExtension.UpdateAllAsync(connection, entityList, batchSize, cancellationToken: token),
            new { entities = entityList, batchSize },
            cancellationToken);
    }

    public int UpdateAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(UpdateAll), connection => DbConnectionExtension.UpdateAll(connection, entityList, qualifiers: keyCheck), new { entities = entityList, keyCheck });
    }

    public Task<int> UpdateAllAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToList();
        return ExecuteAsync(
            nameof(UpdateAllAsync),
            (connection, token) => DbConnectionExtension.UpdateAllAsync(connection, entityList, keyCheck, batchSize, cancellationToken: token),
            new { entities = entityList, keyCheck, batchSize },
            cancellationToken);
    }

    [Obsolete("Use UpdateAll with clearer qualifiers where possible.")]
    public int UpdateAllEx<T>(IEnumerable<T> entities, IEnumerable<Field> keyCheck, string tableName) where T : class
    {
        var entityList = entities.ToList();
        var qualifierList = keyCheck.ToList();
        return Execute(nameof(UpdateAllEx), connection => DbConnectionExtension.UpdateAll(connection, tableName, entityList, qualifiers: qualifierList), new { entities = entityList, keyCheck = qualifierList, tableName });
    }

    public int Merge<T>(T entity) where T : class =>
        Execute(nameof(Merge), connection => (int)DbConnectionExtension.Merge(connection, entity), entity);

    public int Merge<T>(T entity, Expression<Func<T, object>> keyCheck) where T : class =>
        Execute(nameof(Merge), connection => (int)DbConnectionExtension.Merge(connection, entity, qualifiers: keyCheck), new { entity, keyCheck });

    [Obsolete("Use MergeAll with a table name or typed entities.")]
    public int MergeAll(IEnumerable<ExpandoObject> entities) =>
        throw new NotSupportedException("Merging dynamic entities requires an explicit table name.");

    public int MergeAll(IEnumerable<ExpandoObject> entities, string tableName)
    {
        var entityList = entities.ToList();
        return Execute(nameof(MergeAll), connection => DbConnectionExtension.MergeAll(connection, tableName, entities: entityList), new { entities = entityList, tableName });
    }

    public int MergeAll<T>(IEnumerable<T> entities, string tableName) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(MergeAll), connection => DbConnectionExtension.MergeAll(connection, tableName, entities: entityList), new { entities = entityList, tableName });
    }

    public int MergeAll<T>(IEnumerable<T> entities) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(MergeAll), connection => DbConnectionExtension.MergeAll(connection, entityList), entityList);
    }

    public Task<int> MergeAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToList();
        return ExecuteAsync(
            nameof(MergeAllAsync),
            (connection, token) => DbConnectionExtension.MergeAllAsync(connection, entityList, batchSize, cancellationToken: token),
            new { entities = entityList, batchSize },
            cancellationToken);
    }

    public int MergeAll<T>(IEnumerable<T> entities, Expression<Func<T, object>> keyCheck) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(MergeAll), connection => DbConnectionExtension.MergeAll(connection, entityList, keyCheck), new { entities = entityList, keyCheck });
    }

    public int MergeAll(IEnumerable<ExpandoObject> entities, Expression<Func<ExpandoObject, object>> keyCheck, string tableName)
    {
        var entityList = entities.ToList();
        return Execute(nameof(MergeAll), connection => DbConnectionExtension.MergeAll(connection, tableName, entityList, keyCheck), new { entities = entityList, keyCheck, tableName });
    }

    [Obsolete("Use MergeAll with clearer qualifiers where possible.")]
    public int MergeAllEx<T>(IEnumerable<T> entities, IEnumerable<Field> keyCheck, string tableName) where T : class
    {
        var entityList = entities.ToList();
        var qualifierList = keyCheck.ToList();
        return Execute(nameof(MergeAllEx), connection => DbConnectionExtension.MergeAll(connection, tableName, entityList, qualifiers: qualifierList), new { entities = entityList, keyCheck = qualifierList, tableName });
    }

    public Task<int> MergeAllAsync<T>(IEnumerable<T> entities, string tableName, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToList();
        return ExecuteAsync(
            nameof(MergeAllAsync),
            (connection, token) => DbConnectionExtension.MergeAllAsync(connection, tableName, entityList, batchSize, cancellationToken: token),
            new { entities = entityList, tableName, batchSize },
            cancellationToken);
    }

    public int Insert<T>(T entity) where T : class =>
        Execute(nameof(Insert), connection => (int)DbConnectionExtension.Insert(connection, entity), entity);

    public Task<int> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<int>(
            nameof(InsertAsync),
            async (connection, token) => (int)await DbConnectionExtension.InsertAsync(connection, entity, cancellationToken: token).ConfigureAwait(false),
            entity,
            cancellationToken);

    public int InsertAll<T>(IEnumerable<T> entities) where T : class
    {
        var entityList = entities.ToList();
        return Execute(nameof(InsertAll), connection => DbConnectionExtension.InsertAll(connection, entityList), entityList);
    }

    public Task<int> InsertAllAsync<T>(IEnumerable<T> entities, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
    {
        var entityList = entities.ToList();
        return ExecuteAsync(
            nameof(InsertAllAsync),
            (connection, token) => DbConnectionExtension.InsertAllAsync(connection, entityList, batchSize, cancellationToken: token),
            new { entities = entityList, batchSize },
            cancellationToken);
    }

    public bool InsertAll(Dictionary<string, IEnumerable<object>> models)
    {
        ArgumentNullException.ThrowIfNull(models);

        return Execute(
            nameof(InsertAll),
            connection =>
            {
                using var transaction = connection.BeginTransaction();

                foreach (var (tableName, entities) in models)
                {
                    var entityList = entities.ToList();
                    DbConnectionExtension.InsertAll(connection, tableName, entityList, transaction: transaction);
                }

                transaction.Commit();
                return true;
            },
            models);
    }

    public int DeleteAll<T>(string[] keys) where T : class =>
        Execute(nameof(DeleteAll), connection => DbConnectionExtension.DeleteAll<T>(connection, keys), keys);

    public Task<int> DeleteAllAsync<T>(string[] keys, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync(
            nameof(DeleteAllAsync),
            (connection, token) => DbConnectionExtension.DeleteAllAsync<T>(connection, keys, cancellationToken: token),
            keys,
            cancellationToken);

    public int DeleteAll<T>(string tableName) where T : class =>
        Execute(nameof(DeleteAll), connection => DbConnectionExtension.DeleteAll<T>(connection, tableName), tableName);

    public Task<int> DeleteAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync(
            nameof(DeleteAllAsync),
            (connection, token) => DbConnectionExtension.DeleteAllAsync<T>(connection, tableName, cancellationToken: token),
            tableName,
            cancellationToken);

    public Task<int> DeleteAllAsync(string tableName, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            nameof(DeleteAllAsync),
            (connection, token) => DbConnectionExtension.DeleteAllAsync(connection, tableName, cancellationToken: token),
            tableName,
            cancellationToken);
}
