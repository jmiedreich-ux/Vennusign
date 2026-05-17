using RepoDb;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;

/// <summary>
/// SQL Data Access / RepoDB
/// </summary>
namespace DataManager.DataAccess
{
    public interface ISQLDataAccess
    {
        string ConnectionStringName { get; set; }

        int DeleteAll<T>(string[] mappingData) where T : class;
        int DeleteAll<T>(string tableName) where T : class;
        Task<int> DeleteAllAsync(string tableName);

        IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery<U>(string sql, U Param);
        IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery(string sql);
        IEnumerable<T> ExecuteQuery<T, U>(string sql, U Param) where T : class;
        IEnumerable<T> ExecuteQuery<T>(string sql) where T : class;
        IEnumerable<T> ExecuteQuery<T>(string sql, ExpandoObject Param) where T : class;
        Task<IEnumerable<T>> ExecuteQueryAsync<T, U>(string sql, U Param) where T : class;
        IEnumerable<T> ExecuteSQLQuery<T, U>(string sql, U Param) where T : class;
        IEnumerable<T> ExecuteSQLQuery2<T, U>(string sql, U Param);
        List<Field> GetFieldList(string strDelimitedList);
        int Insert<T>(T mappingData) where T : class;
        
        int InsertAll<T>(IEnumerable<T> mappingData) where T : class;
        bool InsertAll(Dictionary<string, IEnumerable<object>> Models);
        
        int Merge<T>(T mappingData) where T : class;
        int Merge<T>(T mappingData, Expression<Func<T, object>> KeyCheck) where T : class;


        int MergeAll<T>(IEnumerable<T> mappingData) where T : class;
        int MergeAll<T>(IEnumerable<T> mappingData, Expression<Func<T, object>> KeyCheck) where T : class;
        int MergeAll(IEnumerable<ExpandoObject> mappingData);
        int MergeAll(IEnumerable<ExpandoObject> mappingData, string Table);
        int MergeAll<T>(IEnumerable<T> mappingData, string Table) where T : class;
        Task<int> MergeAllAsync<T>(IEnumerable<T> mappingData, string Table) where T : class;
        int MergeAllEx<T>(IEnumerable<T> mappingData, IEnumerable<Field> KeyCheck, string strTableName) where T : class;
        T Query<T>(object obj) where T : class;
        IEnumerable<T> Query<T, U>(string TableName, U Param) where T : class;
        IEnumerable<T> Query<T, U>(U Param) where T : class;
        IEnumerable<T> QueryAll<T>() where T : class;
        
        int Update<T>(T obj) where T : class;
        
        int Update<T>(T mappingData, Expression<Func<T, bool>> KeyCheck) where T : class;
        int Update<T>(T obj, IEnumerable<Field> fields) where T : class;
        int Update(string TableName, Dictionary<string, object> RecordData);
        int UpdateAll<T>(IEnumerable<T> mappingData) where T : class;
        int UpdateAll<T>(IEnumerable<T> mappingData, Expression<Func<T, object>> KeyCheck) where T : class;
        int UpdateAllEx<T>(IEnumerable<T> mappingData, IEnumerable<Field> KeyCheck, string strTableName) where T : class;
    } 
}
