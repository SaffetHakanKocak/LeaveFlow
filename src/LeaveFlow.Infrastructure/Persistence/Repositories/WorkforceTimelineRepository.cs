using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Timeline;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class WorkforceTimelineRepository(IDbConnectionFactory connectionFactory) : IWorkforceTimelineRepository
{
    public Task<PagedResult<WorkforceTimelineDataRow>> GetForAdminAsync(
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateParameters(query);
        parameters.Add("ManagerId", query.ManagerId, DbType.Guid);

        return ExecuteAsync(
            StoredProcedureNames.WorkforceTimelineGetForAdmin,
            parameters,
            query,
            cancellationToken);
    }

    public Task<PagedResult<WorkforceTimelineDataRow>> GetForManagerAsync(
        Guid managerId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateParameters(query);
        parameters.Add("ManagerId", managerId, DbType.Guid);

        return ExecuteAsync(
            StoredProcedureNames.WorkforceTimelineGetForManager,
            parameters,
            query,
            cancellationToken);
    }

    private async Task<PagedResult<WorkforceTimelineDataRow>> ExecuteAsync(
        string storedProcedureName,
        DynamicParameters parameters,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = (await connection.QueryAsync<WorkforceTimelineDataRow>(
            new CommandDefinition(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<WorkforceTimelineDataRow>(
            rows,
            query.PageNumber,
            query.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    private static DynamicParameters CreateParameters(WorkforceTimelineQuery query)
    {
        var parameters = new DynamicParameters();
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("ConsultantSearch", query.ConsultantSearch, DbType.String, size: 256);
        parameters.Add("IncludeInactive", query.IncludeInactive, DbType.Boolean);
        parameters.Add("PageNumber", query.PageNumber, DbType.Int32);
        parameters.Add("PageSize", query.PageSize, DbType.Int32);
        return parameters;
    }
}
