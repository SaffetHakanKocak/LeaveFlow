using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class RoleReadRepository(IDbConnectionFactory connectionFactory) : IRoleReadRepository
{
    public async Task<IReadOnlyList<RoleSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var roles = await connection.QueryAsync<RoleSummary>(
            new CommandDefinition(
                StoredProcedureNames.RolesGetAll,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return roles.AsList();
    }
}
