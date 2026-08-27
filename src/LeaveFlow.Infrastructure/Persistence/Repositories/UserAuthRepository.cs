using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class UserAuthRepository(IDbConnectionFactory connectionFactory) : IUserAuthRepository
{
    public async Task<UserAuthRecord?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("NormalizedEmail", normalizedEmail, DbType.String, size: 256);

        return await connection.QuerySingleOrDefaultAsync<UserAuthRecord>(
            new CommandDefinition(
                StoredProcedureNames.UsersGetByNormalizedEmail,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task UpdateLoginSuccessAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);
        parameters.Add("LastLoginAt", lastLoginAtUtc, DbType.DateTime2);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.UsersUpdateLoginSuccess,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<FailedLoginUpdate> RecordFailedLoginAsync(
        Guid userId,
        int maxFailedAccessAttempts,
        int lockoutDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);
        parameters.Add("MaxFailedAccessAttempts", maxFailedAccessAttempts, DbType.Int32);
        parameters.Add("LockoutDurationMinutes", lockoutDurationMinutes, DbType.Int32);

        var update = await connection.QuerySingleOrDefaultAsync<FailedLoginUpdate>(
            new CommandDefinition(
                StoredProcedureNames.UsersRecordFailedLogin,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return update ?? throw new InvalidOperationException("The failed login could not be recorded.");
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);
        parameters.Add("PasswordHash", passwordHash, DbType.String, size: 500);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.UsersUpdatePasswordHash,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
