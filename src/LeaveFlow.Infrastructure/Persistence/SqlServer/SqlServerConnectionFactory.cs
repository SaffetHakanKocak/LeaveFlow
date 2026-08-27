using System.Data.Common;
using LeaveFlow.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace LeaveFlow.Infrastructure.Persistence.SqlServer;

public sealed class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public ValueTask<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The SQL Server connection string is not configured.");
        }

        return OpenConnectionAsync(cancellationToken);
    }

    private async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
