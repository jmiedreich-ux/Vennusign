namespace Vennu.DataAccess.Configuration;

public class VennuDataAccessOptions
{
    public VennuDataAccessOptions(string connectionString) => ConnectionString = connectionString;

    public string ConnectionString { get; }
}
