using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.UnitTests.LeaveRequests;

public sealed class LeaveReviewServiceTests
{
    [Fact]
    public void Overlaps_Should_DetectBoundaryOverlap()
    {
        Assert.True(LeaveConflictDetector.Overlaps(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 3),
            new DateOnly(2026, 9, 3),
            new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void Overlaps_Should_DetectSameDayOverlap()
    {
        Assert.True(LeaveConflictDetector.Overlaps(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public void Overlaps_Should_ReturnFalse_WhenRangesDoNotOverlap()
    {
        Assert.False(LeaveConflictDetector.Overlaps(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 2),
            new DateOnly(2026, 9, 3),
            new DateOnly(2026, 9, 4)));
    }

    [Fact]
    public void Validate_Should_RejectTooLongReviewNote()
    {
        var service = CreateService([RoleNames.Manager], Guid.NewGuid());

        var result = service.Validate(new ReviewDecisionInput(new string('x', 513)));

        Assert.False(result.IsValid);
        Assert.Contains("ReviewNote", result.Errors.Keys);
    }

    [Fact]
    public async Task ApproveAsync_Should_DenyConsultantReviewer()
    {
        var repository = new ReviewStubLeaveRequestRepository { DecisionResult = true };
        var service = CreateService([RoleNames.Consultant], null, repository);

        var result = await service.ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), new ReviewDecisionInput(null));

        Assert.False(result.Succeeded);
        Assert.Equal("Unauthorized", result.ErrorCode);
        Assert.False(repository.DecisionCalled);
    }

    [Fact]
    public async Task ApproveAsync_Should_CallRepository_ForManager()
    {
        var repository = new ReviewStubLeaveRequestRepository { DecisionResult = true };
        var managerId = Guid.NewGuid();
        var service = CreateService([RoleNames.Manager], managerId, repository);

        var result = await service.ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), new ReviewDecisionInput("Looks fine"));

        Assert.True(result.Succeeded);
        Assert.True(repository.DecisionCalled);
        Assert.Equal(managerId, repository.LastManagerId);
    }

    [Fact]
    public async Task ApproveAsync_Should_ReturnFailure_WhenRequestIsNotPendingOrOutOfScope()
    {
        var repository = new ReviewStubLeaveRequestRepository { DecisionResult = false };
        var service = CreateService([RoleNames.Administrator], null, repository);

        var result = await service.ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), new ReviewDecisionInput(null));

        Assert.False(result.Succeeded);
        Assert.Equal("NotPendingOrUnauthorized", result.ErrorCode);
        Assert.True(repository.DecisionCalled);
    }

    private static LeaveReviewService CreateService(
        IReadOnlyList<string> roles,
        Guid? managerId,
        ReviewStubLeaveRequestRepository? repository = null)
    {
        return new LeaveReviewService(
            repository ?? new ReviewStubLeaveRequestRepository(),
            new ReviewStubUserRoleRepository(roles),
            new ReviewStubManagerIdentityRepository(managerId));
    }
}

internal sealed class ReviewStubLeaveRequestRepository : ILeaveRequestRepository
{
    public bool DecisionResult { get; init; }

    public bool DecisionCalled { get; private set; }

    public Guid? LastManagerId { get; private set; }

    public Task<LeaveFlow.Application.Common.PagedResult<LeaveRequestListItem>> GetMineAsync(Guid consultantId, LeaveRequestSearchRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<LeaveRequestDetail?> GetByIdAsync(Guid leaveRequestId, Guid consultantId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<bool> ExistsOverlapAsync(Guid consultantId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<Guid> CreateAsync(Guid consultantId, LeaveRequestInput input, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<LeaveFlow.Application.Common.PagedResult<PendingLeaveRequestListItem>> GetPendingForManagerAsync(Guid managerId, LeaveReviewSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LeaveFlow.Application.Common.PagedResult<PendingLeaveRequestListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<LeaveFlow.Application.Common.PagedResult<PendingLeaveRequestListItem>> GetPendingForAdminAsync(LeaveReviewSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LeaveFlow.Application.Common.PagedResult<PendingLeaveRequestListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<LeaveRequestReviewDetail?> GetForReviewAsync(Guid leaveRequestId, Guid reviewerUserId, Guid? reviewerManagerId, bool isAdministrator, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<LeaveRequestReviewDetail?>(null);
    }

    public Task<IReadOnlyList<LeaveConflict>> GetConflictsAsync(Guid leaveRequestId, Guid reviewerUserId, Guid? reviewerManagerId, bool isAdministrator, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<LeaveConflict>)[]);
    }

    public Task<bool> ApproveAsync(Guid leaveRequestId, Guid reviewerUserId, Guid? reviewerManagerId, bool isAdministrator, ReviewDecisionInput input, CancellationToken cancellationToken = default)
    {
        DecisionCalled = true;
        LastManagerId = reviewerManagerId;
        return Task.FromResult(DecisionResult);
    }

    public Task<bool> RejectAsync(Guid leaveRequestId, Guid reviewerUserId, Guid? reviewerManagerId, bool isAdministrator, ReviewDecisionInput input, CancellationToken cancellationToken = default)
    {
        DecisionCalled = true;
        LastManagerId = reviewerManagerId;
        return Task.FromResult(DecisionResult);
    }
}

internal sealed class ReviewStubUserRoleRepository(IReadOnlyList<string> roles) : IUserRoleRepository
{
    public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(roles);
    }
}

internal sealed class ReviewStubManagerIdentityRepository(Guid? managerId) : IManagerIdentityRepository
{
    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(managerId);
    }

    public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
