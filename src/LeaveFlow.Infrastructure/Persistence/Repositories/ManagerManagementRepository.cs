using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ManagerManagementRepository(IDbConnectionFactory connectionFactory) : IManagerManagementRepository
{
    public async Task<PagedResult<ManagerListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Search", request.Search, DbType.String, size: 256);
        parameters.Add("IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<ManagerListItem>(
            new CommandDefinition(
                StoredProcedureNames.ManagersGetAll,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<ManagerListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<ManagerDetail?> GetByIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<ManagerDetail>(
            new CommandDefinition(
                StoredProcedureNames.ManagersGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(ManagerInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = CreateParameters(input);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.ManagersCreate,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Guid managerId, ManagerInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = CreateParameters(input);
        parameters.Add("ManagerId", managerId, DbType.Guid);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ManagersUpdate,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(Guid managerId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("IsActive", isActive, DbType.Boolean);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ManagersSetActive,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    private static DynamicParameters CreateParameters(ManagerInput input)
    {
        var parameters = new DynamicParameters();
        parameters.Add("FirstName", input.FirstName.Trim(), DbType.String, size: 80);
        parameters.Add("LastName", input.LastName.Trim(), DbType.String, size: 80);
        parameters.Add("Email", input.Email.Trim(), DbType.String, size: 256);
        parameters.Add("Department", string.IsNullOrWhiteSpace(input.Department) ? null : input.Department.Trim(), DbType.String, size: 120);
        parameters.Add("IsActive", input.IsActive, DbType.Boolean);
        return parameters;
    }
}
