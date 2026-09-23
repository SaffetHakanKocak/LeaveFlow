using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ConsultantManagementRepository(IDbConnectionFactory connectionFactory) : IConsultantManagementRepository
{
    public async Task<PagedResult<ConsultantListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Search", request.Search, DbType.String, size: 256);
        parameters.Add("IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<ConsultantListItem>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsGetAll,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<ConsultantListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<ConsultantDetail?> GetByIdAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<ConsultantDetail>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(ConsultantInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = CreateParameters(input);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsCreate,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Guid consultantId, ConsultantInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = CreateParameters(input);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsUpdate,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return await ExistsAsync(connection, consultantId, cancellationToken);
    }

    public async Task<bool> SetActiveAsync(Guid consultantId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        parameters.Add("IsActive", isActive, DbType.Boolean);

        await connection.ExecuteAsync(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsSetActive,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return await ExistsAsync(connection, consultantId, cancellationToken);
    }

    private static DynamicParameters CreateParameters(ConsultantInput input)
    {
        var parameters = new DynamicParameters();
        parameters.Add("FirstName", input.FirstName.Trim(), DbType.String, size: 80);
        parameters.Add("LastName", input.LastName.Trim(), DbType.String, size: 80);
        parameters.Add("Email", input.Email.Trim(), DbType.String, size: 256);
        parameters.Add("EmployeeNumber", string.IsNullOrWhiteSpace(input.EmployeeNumber) ? null : input.EmployeeNumber.Trim(), DbType.String, size: 64);
        parameters.Add("Department", string.IsNullOrWhiteSpace(input.Department) ? null : input.Department.Trim(), DbType.String, size: 120);
        parameters.Add("StartDate", input.StartDate, DbType.Date);
        parameters.Add("IsActive", input.IsActive, DbType.Boolean);
        return parameters;
    }

    private static async Task<bool> ExistsAsync(IDbConnection connection, Guid consultantId, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        var id = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                StoredProcedureNames.ConsultantsGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return id.HasValue;
    }
}
