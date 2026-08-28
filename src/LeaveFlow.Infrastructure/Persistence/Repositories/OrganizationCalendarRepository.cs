using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Calendar;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class OrganizationCalendarRepository(IDbConnectionFactory connectionFactory) : IOrganizationCalendarRepository
{
    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForAdminAsync(
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters(query);
        parameters.Add("ManagerId", query.ManagerId, DbType.Guid);
        parameters.Add("ConsultantId", query.ConsultantId, DbType.Guid);
        return QueryEventsAsync(StoredProcedureNames.OrganizationCalendarGetForAdmin, parameters, cancellationToken);
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForManagerAsync(
        Guid managerId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters(query);
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("ConsultantId", query.ConsultantId, DbType.Guid);
        return QueryEventsAsync(StoredProcedureNames.OrganizationCalendarGetForManager, parameters, cancellationToken);
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForConsultantAsync(
        Guid consultantId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters(query);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        return QueryEventsAsync(StoredProcedureNames.OrganizationCalendarGetForConsultant, parameters, cancellationToken);
    }

    public Task<CalendarEventDetail?> GetDetailForAdminAsync(
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        return QueryDetailAsync(StoredProcedureNames.OrganizationCalendarGetDetailForAdmin, CreateDetailParameters(query), cancellationToken);
    }

    public Task<CalendarEventDetail?> GetDetailForManagerAsync(
        Guid managerId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateDetailParameters(query);
        parameters.Add("ManagerId", managerId, DbType.Guid);
        return QueryDetailAsync(StoredProcedureNames.OrganizationCalendarGetDetailForManager, parameters, cancellationToken);
    }

    public Task<CalendarEventDetail?> GetDetailForConsultantAsync(
        Guid consultantId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateDetailParameters(query);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        return QueryDetailAsync(StoredProcedureNames.OrganizationCalendarGetDetailForConsultant, parameters, cancellationToken);
    }

    private async Task<IReadOnlyList<OrganizationCalendarDataRow>> QueryEventsAsync(
        string storedProcedureName,
        DynamicParameters parameters,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = (await connection.QueryAsync<OrganizationCalendarDataRow>(
            new CommandDefinition(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return rows;
    }

    private async Task<CalendarEventDetail?> QueryDetailAsync(
        string storedProcedureName,
        DynamicParameters parameters,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<CalendarEventDetail>(
            new CommandDefinition(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    private static DynamicParameters CreateQueryParameters(OrganizationCalendarQuery query)
    {
        var parameters = new DynamicParameters();
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        return parameters;
    }

    private static DynamicParameters CreateDetailParameters(CalendarEventDetailQuery query)
    {
        var parameters = new DynamicParameters();
        parameters.Add("EventType", query.EventType, DbType.String, size: 64);
        parameters.Add("EventId", query.EventId, DbType.Guid);
        return parameters;
    }
}
