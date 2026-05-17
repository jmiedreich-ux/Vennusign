using Microsoft.Data.SqlClient;
using Vennu.DataAccess.Configuration;

namespace Vennu.DataAccess.Infrastructure;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly VennuDataAccessOptions options;

    public SqlConnectionFactory(VennuDataAccessOptions options) => this.options = options;

    public SqlConnection CreateConnection() => new(options.ConnectionString);
}
