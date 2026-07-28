using System.Data;
using System.Dynamic;
using RepoDb;

namespace Vennu.DataAccess;

public partial class SqlDataAccess
{
    public bool Exists<T>(object criteria) where T : class =>
        Execute(nameof(Exists), connection => DbConnectionExtension.Exists<T>(connection, criteria), criteria);

    public T? Query<T>(object criteria) where T : class =>
        Execute(nameof(Query), connection => DbConnectionExtension.Query<T>(connection, criteria).FirstOrDefault(), criteria);

    public IEnumerable<T> Query<T, TParameters>(string tableName, TParameters parameters) where T : class =>
        Execute(
            nameof(Query),
            connection => DbConnectionExtension.Query<T, TParameters>(connection, tableName, parameters).ToList(),
            new { tableName, parameters });

    public Task<IEnumerable<T>> QueryAsync<T, TParameters>(string tableName, TParameters parameters, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<IEnumerable<T>>(
            nameof(QueryAsync),
            async (connection, token) => (await DbConnectionExtension.QueryAsync<T, TParameters>(connection, tableName, parameters, cancellationToken: token).ConfigureAwait(false)).ToList(),
            new { tableName, parameters },
            cancellationToken);

    public IEnumerable<T> Query<T, TParameters>(TParameters parameters) where T : class =>
        Execute(nameof(Query), connection => DbConnectionExtension.Query<T, TParameters>(connection, parameters).ToList(), parameters);

    public IEnumerable<T> QueryAll<T>() where T : class =>
        Execute(nameof(QueryAll), connection => DbConnectionExtension.QueryAll<T>(connection).ToList(), typeof(T).Name);

    public Task<T?> QueryAsync<T>(object criteria, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<T?>(
            nameof(QueryAsync),
            async (connection, token) => (await DbConnectionExtension.QueryAsync<T>(connection, criteria, cancellationToken: token).ConfigureAwait(false)).FirstOrDefault(),
            criteria,
            cancellationToken);

    public Task<IEnumerable<T>> QueryAsync<T, TParameters>(TParameters parameters, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<IEnumerable<T>>(
            nameof(QueryAsync),
            async (connection, token) => (await DbConnectionExtension.QueryAsync<T, TParameters>(connection, parameters, cancellationToken: token).ConfigureAwait(false)).ToList(),
            parameters,
            cancellationToken);

    public Task<IEnumerable<T>> QueryAllAsync<T>(CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<IEnumerable<T>>(
            nameof(QueryAllAsync),
            async (connection, token) => (await DbConnectionExtension.QueryAllAsync<T>(connection, cancellationToken: token).ConfigureAwait(false)).ToList(),
            typeof(T).Name,
            cancellationToken);

    public IEnumerable<T> ExecuteQuery<T, TParameters>(string sql, TParameters parameters) where T : class =>
        Execute(
            nameof(ExecuteQuery),
            connection => DbConnectionExtension.ExecuteQuery<T>(connection, sql, parameters, commandType: CommandType.StoredProcedure, commandTimeout: DefaultCommandTimeoutSeconds).ToList(),
            new { sql, parameters });

    public IEnumerable<T> ExecuteQuery<T>(string sql) where T : class =>
        Execute(nameof(ExecuteQuery), connection => DbConnectionExtension.ExecuteQuery<T>(connection, sql).ToList(), sql);

    public IEnumerable<T> ExecuteQuery<T>(string sql, ExpandoObject parameters) where T : class =>
        Execute(
            nameof(ExecuteQuery),
            connection => DbConnectionExtension.ExecuteQuery<T>(connection, sql, param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: DefaultCommandTimeoutSeconds).ToList(),
            new { sql, parameters });

    public Task<IEnumerable<T>> ExecuteQueryAsync<T, TParameters>(string sql, TParameters parameters, CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<IEnumerable<T>>(
            nameof(ExecuteQueryAsync),
            async (connection, token) => (await DbConnectionExtension.ExecuteQueryAsync<T>(connection, sql, parameters, commandType: CommandType.StoredProcedure, commandTimeout: DefaultCommandTimeoutSeconds, cancellationToken: token).ConfigureAwait(false)).ToList(),
            new { sql, parameters },
            cancellationToken);

    public Task<IEnumerable<T>> ExecuteSqlQueryAsync<T, TParameters>(
        string sql,
        TParameters parameters,
        CancellationToken cancellationToken = default) where T : class =>
        ExecuteAsync<IEnumerable<T>>(
            nameof(ExecuteSqlQueryAsync),
            async (connection, token) => (await DbConnectionExtension.ExecuteQueryAsync<T>(
                connection,
                sql,
                parameters,
                commandType: CommandType.Text,
                commandTimeout: DefaultCommandTimeoutSeconds,
                cancellationToken: token).ConfigureAwait(false)).ToList(),
            new { sql, parameters },
            cancellationToken);

    public IEnumerable<T> ExecuteSQLQuery<T, TParameters>(string sql, TParameters parameters) where T : class =>
        Execute(nameof(ExecuteSQLQuery), connection => DbConnectionExtension.ExecuteQuery<T>(connection, sql, parameters).ToList(), new { sql, parameters });

    [Obsolete("Use ExecuteSQLQuery instead.")]
    public IEnumerable<T> ExecuteSQLQuery2<T, TParameters>(string sql, TParameters parameters) =>
        Execute(nameof(ExecuteSQLQuery2), connection => DbConnectionExtension.ExecuteQuery<T>(connection, sql, parameters).ToList(), new { sql, parameters });

    public IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery<TParameters>(string sql, TParameters parameters) =>
        Execute(
            nameof(ExecuteDynamicSQLQuery),
            connection => DbConnectionExtension.ExecuteQuery<ExpandoObject>(connection, sql, parameters, commandTimeout: DynamicQueryTimeoutSeconds).ToList(),
            new { sql, parameters });

    public IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery(string sql) =>
        Execute(nameof(ExecuteDynamicSQLQuery), connection => DbConnectionExtension.ExecuteQuery<ExpandoObject>(connection, sql).ToList(), sql);
}
