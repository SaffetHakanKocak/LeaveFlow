using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(IDbConnectionFactory connectionFactory) : IAuditLogRepository
{
    public async Task InsertAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ActorUserId", record.ActorUserId, DbType.Guid);
        parameters.Add("Action", record.Action, DbType.String, size: 128);
        parameters.Add("TargetType", record.TargetType, DbType.String, size: 128);
        parameters.Add("TargetId", record.TargetId, DbType.String, size: 128);
        parameters.Add("Outcome", record.Outcome, DbType.String, size: 32);
        parameters.Add("CorrelationId", record.CorrelationId, DbType.String, size: 128);
        parameters.Add("MetadataJson", record.MetadataJson, DbType.String, size: 2000);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.AuditLogsInsert,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
