using System.Data.Common;

namespace LeaveFlow.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
