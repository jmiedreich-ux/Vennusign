using Microsoft.Data.SqlClient;

namespace Vennu.DataAccess;

[Obsolete("Use standard connection open semantics instead of pre-flight server checks.")]
public static class SqlConnectionExtensions
{
    public static bool IsAvailable(this SqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        try
        {
            connection.Open();
            connection.Close();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    public static bool CheckServerOnline(this SqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        try
        {
            connection.Open();
            connection.Close();
            return true;
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException("Unable to connect to SQL Server.", ex);
        }
    }
}
