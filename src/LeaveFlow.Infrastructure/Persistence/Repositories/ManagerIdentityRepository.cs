using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ManagerIdentityRepository(IDbConnectionFactory connectionFactory) : IManagerIdentityRepository
{
    public async Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                StoredProcedureNames.ManagersGetIdByUserId,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                StoredProcedureNames.ManagerConsultantsExists,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
