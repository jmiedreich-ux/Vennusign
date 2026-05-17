using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RepoDb;
using Serilog;
using SerilogTimings;
using System.Threading.Tasks;

/// <summary>
/// SQL Data Access w/ RepoDB
/// </summary>
namespace DataManager.DataAccess
{
    public class SQLDataAccess : ISQLDataAccess
    {
        #region Fields
        private readonly IConfiguration AppConfig;
        private readonly string connectionString;
        public string ConnectionStringName { get; set; } = "ConnectionString";
        public string ParamTrace { get; set; } = "N";


        //readonly Serilog.ILogger Log = Log.ForContext<SQLDataAccess>();

        Stopwatch ProcessTracker = new Stopwatch();

        #endregion

        public SQLDataAccess(IConfiguration ConfigurationServices)
        {
            AppConfig = ConfigurationServices;
            //Log = logger;

            connectionString = AppConfig.GetSection(ConnectionStringName).Value;

            SqlServerBootstrap.Initialize();
        }

        // ============================================================================================================================================================
        #region RepoDB Operation Procedures
        // ============================================================================================================================================================
        public bool Exists<T>(object param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    bool Found = connection.Exists<T>(param);
                    Log.Verbose("{0}-{1}-[2] Operation Succesfull. Found {3}", nameof(SQLDataAccess), nameof(QueryAll), nameof(T), Found);
                    return Found;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }

        public IEnumerable<T> QueryAll<T>() where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var Data = connection.QueryAll<T>();
                    Log.Verbose("{0}-{1}-[2] Operation Succesfull. Found Records: {3}", nameof(SQLDataAccess), nameof(QueryAll), nameof(T), Data.Count().ToString());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        
        public T Query<T>(object obj) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var Data = connection.Query<T>(obj).FirstOrDefault();
                    Log.Verbose("Operation Succesfull. Found Record");
                    return (T)Data;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message} / {ex.InnerException}");
                    throw;
                }
            }
        }
        public IEnumerable<T> Query<T, U>(string TableName, U Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var Data = connection.Query<T, U>(TableName, Param);
                    Log.Verbose("Operation Succesfull. Found Record");
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message} / {ex.InnerException}");
                    throw;
                }
            }

        }
        public IEnumerable<T> Query<T, U>(U Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var Data = connection.Query<T, U>(Param);
                    Log.Verbose("Operation Succesfull. Found Record");
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message} / {ex.InnerException}");
                    throw;
                }
            }

        }
        
        // Updates Single Record On Specfic Fields 
        public int Update(string TableName, Dictionary<string, object> RecordData)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0} {1}", RecordData, TableName);
                }

                try
                {
                    var UpdatedRows = connection.Update(TableName, entity: RecordData);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
       
        // Updates Single Record On Specfic Fields 
        public int Update<T>(T obj, IEnumerable<Field> fields) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.Update<T>(entity: obj, fields: fields);
                    Log.Verbose("Operation Succesfull. Updated Records: {UpdatedRows}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int Update<T>(T obj) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.Update<T>(obj);
                    Log.Verbose("Operation Succesfull. Updated Records: {UpdatedRows}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int Update<T>(T mappingData, Expression<Func<T, bool>> KeyCheck) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.Update<T>(mappingData, KeyCheck);
                    Log.Verbose($"Operation Succesfull. Records Updated: {UpdatedRows}");
                    return (int)UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        
        public int UpdateAll<T>(IEnumerable<T> mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.UpdateAll<T>(mappingData);
                    Log.Verbose($"Operation Succesfull. Records Updated: {UpdatedRows}");
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int UpdateAll<T>(IEnumerable<T> mappingData, Expression<Func<T, object>> KeyCheck) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.UpdateAll<T>(mappingData, qualifiers: KeyCheck);
                    Log.Verbose($"Operation Succesfull. Records Updated: {UpdatedRows}");
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int UpdateAllEx<T>(IEnumerable<T> mappingData, IEnumerable<Field> KeyCheck, string strTableName) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.UpdateAll<T>(strTableName, mappingData, qualifiers: KeyCheck);
                    Log.Verbose($"Operation Succesfull. Records Updated: {UpdatedRows}");
                    return UpdatedRows;
                    
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }

        public int Merge<T>(T mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var PrimaryKeyReturned = connection.Merge<T>(mappingData);
                    Log.Verbose($"Operation Succesfull. Primary Key Returned: {PrimaryKeyReturned}");
                    return (int)PrimaryKeyReturned;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int Merge<T>(T mappingData, Expression<Func<T, object>> KeyCheck) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();

                    var PrimaryKeyReturned = connection.Merge<T>(mappingData, qualifiers: KeyCheck);
                    Log.Verbose($"Operation Succesfull. Primary Key Returned: {PrimaryKeyReturned}");
                    return (int)PrimaryKeyReturned;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int MergeAll(IEnumerable<ExpandoObject> mappingData)
        {
            throw new NotImplementedException();
        }

        
        
        public int MergeAll<T>(IEnumerable<T> mappingData, string Table) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Table);
                }

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.MergeAll<T>(Table, entities: mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        public int MergeAll(IEnumerable<ExpandoObject> mappingData, string Table)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Table);
                }

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.MergeAll(Table, entities: mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        public int MergeAll<T>(IEnumerable<T> mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.MergeAll<T>(mappingData);
                    Log.Verbose("Operation Succesfull. Updated Records: {UpdatedRows}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int MergeAll<T>(IEnumerable<T> mappingData, Expression<Func<T, object>> KeyCheck) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.MergeAll<T>(mappingData, KeyCheck);
                    Log.Verbose("Operation Succesfull. Updated Records: {UpdatedRows}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int MergeAll(IEnumerable<ExpandoObject> mappingData, Expression<Func<ExpandoObject, object>> KeyCheck, string Table)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.MergeAll(Table, mappingData, KeyCheck);
                    Log.Verbose("Operation Succesfull. Updated Records: {UpdatedRows}", UpdatedRows);
                    return UpdatedRows;
                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }
        public int MergeAllEx<T>(IEnumerable<T> mappingData, IEnumerable<Field> KeyCheck, string strTableName) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.CheckServerOnline();
                    var UpdatedRows = connection.MergeAll<T>(strTableName, mappingData, qualifiers: KeyCheck);
                    Log.Verbose($"Operation Succesfull. Records Updated: {UpdatedRows}");
                    return UpdatedRows;

                }
                catch (System.Exception ex)
                {
                    Log.Fatal($"Database Exception Occured {ex.Message}");
                    throw;
                }
            }
        }


        public async Task<int> MergeAllAsync<T>(IEnumerable<T> mappingData, string Table) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Table);
                }

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.MergeAll<T>(Table, entities: mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }

        public int Insert<T>(T mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();
                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.Insert<T>(mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecordds", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return (int)UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        
        public int InsertAll<T>(IEnumerable<T> mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.InsertAll<T>(mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecordds", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        public bool InsertAll(Dictionary<string, IEnumerable<object>> Models)
        {
            StringBuilder Result = new StringBuilder();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var transaction = connection.EnsureOpen().BeginTransaction())
                {
                    try
                    {
                        using (var op = Operation.Begin("Database Operation Insert Multiple Model"))
                        {
                            foreach (var Model in Models)
                            {
                                var Table = Model.Key;
                                var DataToInsert = Model.Value;
                                using (var op2 = Operation.Begin("Database Operation Insert Multiple Model"))
                                {
                                    var InsertedRows = connection.InsertAll(Table, entities: DataToInsert, transaction: transaction);
                                    op2.EnrichWith("Method", GetActualAsyncMethodName());
                                    op2.EnrichWith("SqlRecordds", InsertedRows);
                                    op2.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                                }
                            }

                            transaction.Commit();

                            op.EnrichWith("Method", GetActualAsyncMethodName());
                            op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                        throw;
                    }
                }
            }
        }
        
        public int DeleteAll<T>(string[] mappingData) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();
                
                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.DeleteAll<T>(mappingData);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
                public int DeleteAll<T>(string tableName) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.DeleteAll<T>(tableName);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }

        public async Task<int> DeleteAllAsync(string tableName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                try
                {
                    using (var op = Operation.Begin("Database Operation"))
                    {
                        var UpdatedRows = connection.DeleteAll(tableName);
                        op.EnrichWith("Method", GetActualAsyncMethodName());
                        op.EnrichWith("SqlRecords", UpdatedRows);
                        op.Complete("ProcessTracker", Math.Round(op.Elapsed.TotalSeconds, 2));
                        return UpdatedRows;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }


        #endregion

        // ============================================================================================================================================================

        #region SQL Stored Procedures
        public IEnumerable<T> ExecuteQuery<T, U>(string sql, U Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Param);
                }

                try
                {
                    var Data = connection.ExecuteQuery<T>(sql, Param, commandType: CommandType.StoredProcedure, commandTimeout: 180);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    Log.Fatal(ex.Message);
                    throw;
                }
            }
        }
        public IEnumerable<T> ExecuteQuery<T>(string sql, ExpandoObject Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Param);
                }

                try
                {
                    var Data = connection.ExecuteQuery<T>(sql, param: Param, commandType: CommandType.StoredProcedure, commandTimeout: 180);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }


        public async Task<IEnumerable<T>> ExecuteQueryAsync<T, U>(string sql, U Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Param);
                }

                try
                {
                    var Data = connection.ExecuteQuery<T>(sql, Param, commandType: CommandType.StoredProcedure, commandTimeout: 180);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Fatal("Database Exception Occured", ex.Message);
                    Log.Fatal(ex.Message);
                    throw;
                }
            }
        }




        #endregion

        // ============================================================================================================================================================
        #region Custom SQL Statements

        public IEnumerable<T> ExecuteQuery<T>(string sql) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();
                try
                {
                    var Data = connection.ExecuteQuery<T>(sql);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Error("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }

        public IEnumerable<T> ExecuteSQLQuery2<T, U>(string sql, U Param)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();
                try
                {
                    var Data = connection.ExecuteQuery<T>(sql, Param);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Error("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }

        public IEnumerable<T> ExecuteSQLQuery<T, U>(string sql, U Param) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Param);
                }

                try
                {
                    var Data = connection.ExecuteQuery<T>(sql, Param);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Error("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        public IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery<U>(string sql, U Param)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();

                if (ParamTrace == "Y")
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Information("Param: {0}", Param);
                }

                try
                {
                    var Data = connection.ExecuteQuery<ExpandoObject>(sql, Param, null, null, null, "60");
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
                }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Error("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
        public IEnumerable<ExpandoObject> ExecuteDynamicSQLQuery(string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.CheckServerOnline();
                try
                {
                    var Data = connection.ExecuteQuery<ExpandoObject>(sql);
                    Log.ForContext("Method", GetActualAsyncMethodName()).Verbose("Database Operation Completed. Records: {0}", Data.Count());
                    return Data;
            }
                catch (System.Exception ex)
                {
                    Log.ForContext("Method", GetActualAsyncMethodName()).Error("Database Exception Occured", ex.Message);
                    throw;
                }
            }
        }
            

        #endregion

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public List<Field> GetFieldList(string strDelimitedList)
        {
            if (String.IsNullOrEmpty(strDelimitedList))
            {
                return null;
            }
            
            List<Field> fieldList = new List<Field>();

            var fields = strDelimitedList.Split("|");

            for (int i = 0; i < fields.Length; i++)
            {
                Field pkField = new Field(fields[i]);
                fieldList.Add(pkField);
            }

            return fieldList;
        }
    }
}
