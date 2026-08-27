using LeaveFlow.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace LeaveFlow.Infrastructure.Persistence.SqlServer;

public sealed class SqlDataTransactionFactory(string connectionString) : IDataTransactionFactory
{
    public async Task<IDataTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The SQL Server connection string is not configured.");
        }

        var connection = new SqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            return new SqlDataTransaction(connection, transaction, cancellationToken);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
