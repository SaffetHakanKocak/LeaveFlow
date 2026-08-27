using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class LeaveRequestRepository(IDbConnectionFactory connectionFactory) : ILeaveRequestRepository
{
    public async Task<PagedResult<LeaveRequestListItem>> GetMineAsync(
        Guid consultantId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        parameters.Add("Status", request.Status, DbType.String, size: 32);
        parameters.Add("FromDate", request.FromDate, DbType.Date);
        parameters.Add("ToDate", request.ToDate, DbType.Date);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<LeaveRequestListItem>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsGetMine,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<LeaveRequestListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<LeaveRequestDetail?> GetByIdAsync(
        Guid leaveRequestId,
        Guid consultantId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("LeaveRequestId", leaveRequestId, DbType.Guid);
        parameters.Add("ConsultantId", consultantId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<LeaveRequestDetail>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsGetById,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsOverlapAsync(
        Guid consultantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        parameters.Add("StartDate", startDate, DbType.Date);
        parameters.Add("EndDate", endDate, DbType.Date);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsExistsOverlap,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(
        Guid consultantId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        parameters.Add("Reason", input.Reason.Trim(), DbType.String, size: 512);
        parameters.Add("StartDate", input.StartDate, DbType.Date);
        parameters.Add("EndDate", input.EndDate, DbType.Date);

        return await connection.QuerySingleAsync<Guid>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsCreate,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }
}
