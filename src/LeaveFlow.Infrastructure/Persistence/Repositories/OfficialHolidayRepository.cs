using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class OfficialHolidayRepository(IDbConnectionFactory connectionFactory) : IOfficialHolidayRepository
{
    private const string GenericCountryCode = "ZZ";

    public async Task<PagedResult<OfficialHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Search", request.Search, DbType.String, size: 256);
        parameters.Add("FromDate", request.FromDate, DbType.Date);
        parameters.Add("ToDate", request.ToDate, DbType.Date);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<OfficialHolidayListItem>(
            new CommandDefinition(
                StoredProcedureNames.OfficialHolidayDefinitionsGetAll,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<OfficialHolidayListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<OfficialHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("OfficialHolidayDefinitionId", holidayId, DbType.Guid);

        using var grid = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.OfficialHolidayDefinitionsGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var detail = await grid.ReadSingleOrDefaultAsync<OfficialHolidayDetailRow>();
        if (detail is null)
        {
            return null;
        }

        var days = (await grid.ReadAsync<DateOnly>()).AsList();
        return new OfficialHolidayDetail(detail.Id, detail.Name, detail.StartDate, detail.EndDate, days);
    }

    public Task<Guid> CreateAsync(OfficialHolidayInput input, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(
            StoredProcedureNames.OfficialHolidayDefinitionsCreate,
            CreateChangeParameters(input),
            command => command.Connection.QuerySingleAsync<Guid>(command),
            cancellationToken);
    }

    public Task<bool> UpdateAsync(Guid holidayId, OfficialHolidayInput input, CancellationToken cancellationToken = default)
    {
        var parameters = CreateChangeParameters(input);
        parameters.Add("OfficialHolidayDefinitionId", holidayId, DbType.Guid);

        return ExecuteInTransactionAsync(
            StoredProcedureNames.OfficialHolidayDefinitionsUpdate,
            parameters,
            command => command.Connection.QuerySingleAsync<bool>(command),
            cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("OfficialHolidayDefinitionId", holidayId, DbType.Guid);

        return ExecuteInTransactionAsync(
            StoredProcedureNames.OfficialHolidayDefinitionsDelete,
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

    private static DynamicParameters CreateChangeParameters(OfficialHolidayInput input)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Name", input.Name.Trim(), DbType.String, size: 160);
        parameters.Add("StartDate", input.StartDate, DbType.Date);
        parameters.Add("EndDate", input.EndDate, DbType.Date);
        parameters.Add("CountryCode", GenericCountryCode, DbType.StringFixedLength, size: 2);
        parameters.Add("RegionCode", null, DbType.String, size: 32);
        return parameters;
    }

    private sealed record OfficialHolidayDetailRow(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate);

    private sealed record CommandDefinitionWithConnection(IDbConnection Connection, CommandDefinition Command)
    {
        public static implicit operator CommandDefinition(CommandDefinitionWithConnection value) => value.Command;
    }
}
