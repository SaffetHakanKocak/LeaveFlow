using System.Data.Common;
using LeaveFlow.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace LeaveFlow.Infrastructure.Persistence.SqlServer;

internal sealed class SqlDataTransaction(
    SqlConnection connection,
    SqlTransaction transaction,
    CancellationToken cancellationToken) : IDataTransaction
{
    public CancellationToken CancellationToken => cancellationToken;

    internal DbConnection Connection => connection;

    internal DbTransaction Transaction => transaction;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
        await connection.DisposeAsync();
    }
}
