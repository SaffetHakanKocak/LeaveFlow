using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;

namespace LeaveFlow.Application.Abstractions.LeaveRequests;

public interface ILeaveRequestRepository
{
    Task<PagedResult<LeaveRequestListItem>> GetMineAsync(
        Guid consultantId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestDetail?> GetByIdAsync(
        Guid leaveRequestId,
        Guid consultantId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsOverlapAsync(
        Guid consultantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        Guid consultantId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForManagerAsync(
        Guid managerId,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForAdminAsync(
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestReviewDetail?> GetForReviewAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveConflict>> GetConflictsAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default);
}
