using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.People;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ManagerConsultantAssignmentRepository(IDbConnectionFactory connectionFactory) : IManagerConsultantAssignmentRepository
{
    public async Task<IReadOnlyList<ManagerConsultantAssignment>> GetByManagerIdAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);

        var assignments = await connection.QueryAsync<ManagerConsultantAssignment>(
            new CommandDefinition(
                StoredProcedureNames.ManagerConsultantsGetByManagerId,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return assignments.AsList();
    }

    public async Task<bool> AssignAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = CreateParameters(managerId, consultantId);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ManagerConsultantsAssign,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> RemoveAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = CreateParameters(managerId, consultantId);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ManagerConsultantsRemove,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    private static DynamicParameters CreateParameters(Guid managerId, Guid consultantId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        return parameters;
    }
}
