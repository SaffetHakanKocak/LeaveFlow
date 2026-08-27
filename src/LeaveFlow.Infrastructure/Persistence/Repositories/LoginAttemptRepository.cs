using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class LoginAttemptRepository(IDbConnectionFactory connectionFactory) : ILoginAttemptRepository
{
    public async Task InsertAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", record.UserId, DbType.Guid);
        parameters.Add("NormalizedEmail", record.NormalizedEmail, DbType.String, size: 256);
        parameters.Add("IpAddress", record.IpAddress, DbType.String, size: 64);
        parameters.Add("Succeeded", record.Succeeded, DbType.Boolean);
        parameters.Add("FailureReason", record.FailureReason, DbType.String, size: 128);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.LoginAttemptsInsert,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
