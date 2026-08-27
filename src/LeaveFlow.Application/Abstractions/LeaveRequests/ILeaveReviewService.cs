using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.LeaveRequests;

public interface ILeaveReviewService
{
    Task<PagedResult<PendingLeaveRequestListItem>?> GetPendingAsync(
        Guid reviewerUserId,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestReviewDetail?> GetForReviewAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveConflict>?> GetConflictsAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default);

    Task<ReviewDecisionResult> ApproveAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default);

    Task<ReviewDecisionResult> RejectAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default);

    ValidationResult Validate(ReviewDecisionInput input);
}
