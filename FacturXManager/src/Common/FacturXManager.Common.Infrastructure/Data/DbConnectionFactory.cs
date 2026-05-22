
using System.Data.Common;
using FacturXManager.Common.Application.Data;
using Microsoft.Data.SqlClient;

namespace FacturXManager.Common.Infrastructure.Data;
internal sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
