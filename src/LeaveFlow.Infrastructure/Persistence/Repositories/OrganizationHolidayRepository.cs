using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class OrganizationHolidayRepository(IDbConnectionFactory connectionFactory) : IOrganizationHolidayRepository
{
    public async Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = CreateSearchParameters(request);

        var rows = (await connection.QueryAsync<OrganizationHolidayListItem>(
            new CommandDefinition(
                StoredProcedureNames.HolidayDefinitionsGetAll,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<OrganizationHolidayListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<OrganizationHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("HolidayDefinitionId", holidayId, DbType.Guid);

        using var grid = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.HolidayDefinitionsGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var detail = await grid.ReadSingleOrDefaultAsync<OrganizationHolidayDetailRow>();
        if (detail is null)
        {
            return null;
        }

        var days = (await grid.ReadAsync<DateOnly>()).AsList();
        return new OrganizationHolidayDetail(detail.Id, detail.Name, detail.StartDate, detail.EndDate, detail.IsActive, days);
    }

    public Task<Guid> CreateAsync(OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(
            StoredProcedureNames.HolidayDefinitionsCreate,
            CreateChangeParameters(input),
            command => command.Connection.QuerySingleAsync<Guid>(command),
            cancellationToken);
    }

    public Task<bool> UpdateAsync(Guid holidayId, OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        var parameters = CreateChangeParameters(input);
        parameters.Add("HolidayDefinitionId", holidayId, DbType.Guid);

        return ExecuteInTransactionAsync(
            StoredProcedureNames.HolidayDefinitionsUpdate,
            parameters,
            command => command.Connection.QuerySingleAsync<bool>(command),
            cancellationToken);
    }

    public Task<bool> SetActiveAsync(Guid holidayId, bool isActive, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("HolidayDefinitionId", holidayId, DbType.Guid);
        parameters.Add("IsActive", isActive, DbType.Boolean);

        return ExecuteInTransactionAsync(
            StoredProcedureNames.HolidayDefinitionsSetActive,
            parameters,
            command => command.Connection.QuerySingleAsync<bool>(command),
            cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("HolidayDefinitionId", holidayId, DbType.Guid);

        return ExecuteInTransactionAsync(
            StoredProcedureNames.HolidayDefinitionsDelete,
            parameters,
            command => command.Connection.QuerySingleAsync<bool>(command),
            cancellationToken);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(
        string storedProcedureName,
        DynamicParameters parameters,
        Func<CommandDefinitionWithConnection, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var command = new CommandDefinition(
                storedProcedureName,
                parameters,
                transaction: transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var result = await operation(new CommandDefinitionWithConnection(connection, command));
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static DynamicParameters CreateSearchParameters(HolidaySearchRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Search", request.Search, DbType.String, size: 256);
        parameters.Add("IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("FromDate", request.FromDate, DbType.Date);
        parameters.Add("ToDate", request.ToDate, DbType.Date);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);
        return parameters;
    }

    private static DynamicParameters CreateChangeParameters(OrganizationHolidayInput input)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Name", input.Name.Trim(), DbType.String, size: 160);
        parameters.Add("StartDate", input.StartDate, DbType.Date);
        parameters.Add("EndDate", input.EndDate, DbType.Date);
        parameters.Add("IsActive", input.IsActive, DbType.Boolean);
        return parameters;
    }

    private sealed record OrganizationHolidayDetailRow(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive);

    private sealed record CommandDefinitionWithConnection(IDbConnection Connection, CommandDefinition Command)
    {
        public static implicit operator CommandDefinition(CommandDefinitionWithConnection value) => value.Command;
    }
}
