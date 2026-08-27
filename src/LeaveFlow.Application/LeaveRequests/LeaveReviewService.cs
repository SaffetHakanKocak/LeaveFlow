using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Domain.LeaveRequests;

namespace LeaveFlow.Application.LeaveRequests;

public sealed class LeaveReviewService(
    ILeaveRequestRepository repository,
    IUserRoleRepository userRoleRepository,
    IManagerIdentityRepository managerIdentityRepository) : ILeaveReviewService
{
    public async Task<PagedResult<PendingLeaveRequestListItem>?> GetPendingAsync(
        Guid reviewerUserId,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var scope = await ResolveScopeAsync(reviewerUserId, cancellationToken);
        if (!scope.CanReview)
        {
            return null;
        }

        var normalized = Normalize(request);
        return scope.IsAdministrator
            ? await repository.GetPendingForAdminAsync(normalized, cancellationToken)
            : await repository.GetPendingForManagerAsync(scope.ManagerId!.Value, normalized, cancellationToken);
    }

    public async Task<LeaveRequestReviewDetail?> GetForReviewAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default)
    {
        var scope = await ResolveScopeAsync(reviewerUserId, cancellationToken);
        return !scope.CanReview
            ? null
            : await repository.GetForReviewAsync(leaveRequestId, reviewerUserId, scope.ManagerId, scope.IsAdministrator, cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveConflict>?> GetConflictsAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default)
    {
        var scope = await ResolveScopeAsync(reviewerUserId, cancellationToken);
        return !scope.CanReview
            ? null
            : await repository.GetConflictsAsync(leaveRequestId, reviewerUserId, scope.ManagerId, scope.IsAdministrator, cancellationToken);
    }

    public async Task<ReviewDecisionResult> ApproveAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        if (!Validate(input).IsValid)
        {
            return ReviewDecisionResult.Failure("ValidationFailed");
        }

        var scope = await ResolveScopeAsync(reviewerUserId, cancellationToken);
        if (!scope.CanReview)
        {
            return ReviewDecisionResult.Failure("Unauthorized");
        }

        var approved = await repository.ApproveAsync(leaveRequestId, reviewerUserId, scope.ManagerId, scope.IsAdministrator, input, cancellationToken);
        return approved ? ReviewDecisionResult.Success() : ReviewDecisionResult.Failure("NotPendingOrUnauthorized");
    }

    public async Task<ReviewDecisionResult> RejectAsync(
        Guid reviewerUserId,
        Guid leaveRequestId,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        if (!Validate(input).IsValid)
        {
            return ReviewDecisionResult.Failure("ValidationFailed");
        }

        var scope = await ResolveScopeAsync(reviewerUserId, cancellationToken);
        if (!scope.CanReview)
        {
            return ReviewDecisionResult.Failure("Unauthorized");
        }

        var rejected = await repository.RejectAsync(leaveRequestId, reviewerUserId, scope.ManagerId, scope.IsAdministrator, input, cancellationToken);
        return rejected ? ReviewDecisionResult.Success() : ReviewDecisionResult.Failure("NotPendingOrUnauthorized");
    }

    public ValidationResult Validate(ReviewDecisionInput input)
    {
        return LeaveReviewValidation.Validate(input);
    }

    private async Task<ReviewerScope> ResolveScopeAsync(Guid reviewerUserId, CancellationToken cancellationToken)
    {
        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(reviewerUserId, cancellationToken);
        var isAdministrator = roles.Contains(RoleNames.Administrator, StringComparer.Ordinal);
        if (isAdministrator)
        {
            return new ReviewerScope(true, null, true);
        }

        if (!roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            return new ReviewerScope(false, null, false);
        }

        var managerId = await managerIdentityRepository.GetIdByUserIdAsync(reviewerUserId, cancellationToken);
        return new ReviewerScope(managerId is not null, managerId, false);
    }

    private static LeaveReviewSearchRequest Normalize(LeaveReviewSearchRequest request)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? LeaveRequestStatuses.Pending : request.Status.Trim();
        if (!LeaveRequestStatuses.All.Contains(status))
        {
            status = LeaveRequestStatuses.Pending;
        }

        return request with
        {
            Status = status,
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }

    private sealed record ReviewerScope(bool CanReview, Guid? ManagerId, bool IsAdministrator);
}
