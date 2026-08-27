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

    public Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForManagerAsync(
        Guid managerId,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateReviewSearchParameters(request);
        parameters.Add("ManagerId", managerId, DbType.Guid);
        return SearchForReviewAsync(StoredProcedureNames.LeaveRequestsGetPendingForManager, parameters, request, cancellationToken);
    }

    public Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForAdminAsync(
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return SearchForReviewAsync(
            StoredProcedureNames.LeaveRequestsGetPendingForAdmin,
            CreateReviewSearchParameters(request),
            request,
            cancellationToken);
    }

    public async Task<LeaveRequestReviewDetail?> GetForReviewAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<LeaveRequestReviewDetail>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsGetForReview,
                CreateReviewScopeParameters(leaveRequestId, reviewerUserId, reviewerManagerId, isAdministrator),
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<LeaveConflict>> GetConflictsAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<LeaveConflict>(
            new CommandDefinition(
                StoredProcedureNames.LeaveRequestsGetConflicts,
                CreateReviewScopeParameters(leaveRequestId, reviewerUserId, reviewerManagerId, isAdministrator),
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    public Task<bool> ApproveAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        return ExecuteReviewDecisionAsync(
            StoredProcedureNames.LeaveRequestsApprove,
            leaveRequestId,
            reviewerUserId,
            reviewerManagerId,
            isAdministrator,
            input,
            cancellationToken);
    }

    public Task<bool> RejectAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        return ExecuteReviewDecisionAsync(
            StoredProcedureNames.LeaveRequestsReject,
            leaveRequestId,
            reviewerUserId,
            reviewerManagerId,
            isAdministrator,
            input,
            cancellationToken);
    }

    private async Task<PagedResult<PendingLeaveRequestListItem>> SearchForReviewAsync(
        string storedProcedureName,
        DynamicParameters parameters,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = (await connection.QueryAsync<PendingLeaveRequestListItem>(
            new CommandDefinition(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).AsList();

        return new PagedResult<PendingLeaveRequestListItem>(
            rows,
            request.PageNumber,
            request.PageSize,
            rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    private async Task<bool> ExecuteReviewDecisionAsync(
        string storedProcedureName,
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = CreateReviewScopeParameters(leaveRequestId, reviewerUserId, reviewerManagerId, isAdministrator);
        parameters.Add("ReviewNote", string.IsNullOrWhiteSpace(input.ReviewNote) ? null : input.ReviewNote.Trim(), DbType.String, size: 512);

        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    private static DynamicParameters CreateReviewSearchParameters(LeaveReviewSearchRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", request.ConsultantId, DbType.Guid);
        parameters.Add("FromDate", request.FromDate, DbType.Date);
        parameters.Add("ToDate", request.ToDate, DbType.Date);
        parameters.Add("Status", request.Status, DbType.String, size: 32);
        parameters.Add("PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("PageSize", request.PageSize, DbType.Int32);
        return parameters;
    }

    private static DynamicParameters CreateReviewScopeParameters(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator)
    {
        var parameters = new DynamicParameters();
        parameters.Add("LeaveRequestId", leaveRequestId, DbType.Guid);
        parameters.Add("ReviewerUserId", reviewerUserId, DbType.Guid);
        parameters.Add("ReviewerManagerId", reviewerManagerId, DbType.Guid);
        parameters.Add("IsAdministrator", isAdministrator, DbType.Boolean);
        return parameters;
    }
}
