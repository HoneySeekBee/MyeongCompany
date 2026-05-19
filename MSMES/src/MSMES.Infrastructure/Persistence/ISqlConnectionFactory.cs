using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MSMES.Infrastructure.Persistence;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MSMES")
            ?? throw new InvalidOperationException("Missing connection string 'MSMES'.");
    }

    public IDbConnection Create() => new SqlConnection(_connectionString);
}
