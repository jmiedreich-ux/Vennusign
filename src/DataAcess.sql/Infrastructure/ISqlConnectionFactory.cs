using Microsoft.Data.SqlClient;

namespace Vennu.DataAccess.Infrastructure;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
