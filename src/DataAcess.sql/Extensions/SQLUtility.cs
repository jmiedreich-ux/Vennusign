using System;

using System.Reflection;
using Microsoft.Data.SqlClient;
using Serilog;

namespace DataManager.DataAccess
{
    public static class SQLUtility
    {
        static MethodBase m = MethodBase.GetCurrentMethod();
        static string MethodNamespace = m.ReflectedType.Namespace;

        public static bool IsAvailable(this SqlConnection conn)
        {
            try
            {
                conn.Open();
                conn.Close();
                //Log.Debug("SQL Server Is Available");
            }
            catch (SqlException ex)
            {
                return false;
            }
            return true;
        }

        public static bool CheckServerOnline(this SqlConnection conn)
        {
            try
            {
                conn.Open();
                conn.Close();
                //Log.Verbose("SQL Server Found Online", MethodNamespace);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception(string.Format("SQL Exception: {0}", ex.Message));
            }
            
        }
    }
}
