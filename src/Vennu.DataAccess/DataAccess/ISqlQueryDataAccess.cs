using System.Collections.Generic;

namespace Vennu.DataAccess;

public interface ISqlQueryDataAccess
{
    T? Query<T>(object criteria) where T : class;

    IEnumerable<T> Query<T, TParameters>(string tableName, TParameters parameters) where T : class;

    IEnumerable<T> Query<T, TParameters>(TParameters parameters) where T : class;

    IEnumerable<T> QueryAll<T>() where T : class;

    Task<T?> QueryAsync<T>(object criteria, CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<T>> QueryAsync<T, TParameters>(string tableName, TParameters parameters, CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<T>> QueryAsync<T, TParameters>(TParameters parameters, CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<T>> QueryAllAsync<T>(CancellationToken cancellationToken = default) where T : class;
}
