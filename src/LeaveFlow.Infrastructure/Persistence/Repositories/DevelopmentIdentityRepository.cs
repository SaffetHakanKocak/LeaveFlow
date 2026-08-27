using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class DevelopmentIdentityRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<Guid> UpsertUserAsync(
        string email,
        string normalizedEmail,
        string displayName,
        string passwordHash,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Email", email, DbType.String, size: 256);
        parameters.Add("NormalizedEmail", normalizedEmail, DbType.String, size: 256);
        parameters.Add("DisplayName", displayName, DbType.String, size: 160);
        parameters.Add("PasswordHash", passwordHash, DbType.String, size: 500);
        parameters.Add("IsActive", isActive, DbType.Boolean);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.UsersUpsert,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task EnsureRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);
        parameters.Add("RoleName", roleName, DbType.String, size: 64);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.UserRolesEnsure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<Guid> EnsureConsultantForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsEnsureForUser,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<Guid> EnsureManagerForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.ManagersEnsureForUser,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task EnsureManagerConsultantAssignmentAsync(
        Guid managerId,
        Guid consultantId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ManagerConsultantsEnsure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
