using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository(IDbConnectionFactory connectionFactory) : IUserRoleRepository
{
    public async Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);

        var roles = await connection.QueryAsync<string>(
            new CommandDefinition(
                StoredProcedureNames.UserRolesGetByUserId,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return roles.AsList();
    }
}
