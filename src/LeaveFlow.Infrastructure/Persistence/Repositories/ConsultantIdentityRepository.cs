using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ConsultantIdentityRepository(IDbConnectionFactory connectionFactory) : IConsultantIdentityRepository
{
    public async Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsGetIdByUserId,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
