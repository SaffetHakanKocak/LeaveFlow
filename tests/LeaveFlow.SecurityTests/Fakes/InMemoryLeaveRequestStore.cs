using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Domain.LeaveRequests;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryLeaveRequestStore : ILeaveRequestRepository
{
    private readonly Dictionary<Guid, LeaveRequestDetail> _requests = new();
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _reviewScopes = [];
    private readonly HashSet<(Guid ConsultantId, Guid LeaveRequestId, DateOnly LeaveDate)> _dayRows = [];
    private readonly object _gate = new();

    public int LeaveDayRowCount
    {
        get
        {
            lock (_gate)
            {
                return _dayRows.Count;
            }
        }
    }

    public void AssignReviewer(Guid managerId, Guid consultantId)
    {
        _reviewScopes.Add((managerId, consultantId));
    }

    public LeaveRequestDetail Add(
        Guid consultantId,
        string reason,
        DateOnly startDate,
        DateOnly endDate,
        string status = LeaveRequestStatuses.Pending)
    {
        var detail = new LeaveRequestDetail(
            Guid.NewGuid(),
            consultantId,
            reason,
            startDate,
            endDate,
            status,
            DateTime.UtcNow,
            null,
            null,
            null);

        lock (_gate)
        {
            _requests[detail.Id] = detail;
            if (status == LeaveRequestStatuses.Approved)
            {
                AddDayRows(detail);
            }
        }

        return detail;
    }

    public Task<PagedResult<LeaveRequestListItem>> GetMineAsync(
        Guid consultantId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        LeaveRequestDetail[] items;
        lock (_gate)
        {
            items = _requests.Values
                .Where(item => item.ConsultantId == consultantId)
                .Where(item => request.Status is null || item.Status == request.Status)
                .Where(item => request.FromDate is null || item.EndDate >= request.FromDate)
                .Where(item => request.ToDate is null || item.StartDate <= request.ToDate)
                .OrderByDescending(item => item.CreatedAt)
                .ToArray();
        }

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new LeaveRequestListItem(
                item.Id,
                item.ConsultantId,
                item.Reason,
                item.StartDate,
                item.EndDate,
                item.Status,
                item.CreatedAt,
                items.Length))
            .ToArray();

        return Task.FromResult(new PagedResult<LeaveRequestListItem>(page, request.PageNumber, request.PageSize, items.Length));
    }

    public Task<LeaveRequestDetail?> GetByIdAsync(
        Guid leaveRequestId,
        Guid consultantId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(
                _requests.TryGetValue(leaveRequestId, out var request) && request.ConsultantId == consultantId
                    ? request
                    : null);
        }
    }

    public Task<bool> ExistsOverlapAsync(
        Guid consultantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        bool exists;
        lock (_gate)
        {
            exists = _requests.Values.Any(request =>
                request.ConsultantId == consultantId
                && (request.Status == LeaveRequestStatuses.Pending || request.Status == LeaveRequestStatuses.Approved)
                && request.StartDate <= endDate
                && request.EndDate >= startDate);
        }

        return Task.FromResult(exists);
    }

    public Task<Guid> CreateAsync(
        Guid consultantId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default)
    {
        var request = Add(
            consultantId,
            input.Reason,
            input.StartDate.GetValueOrDefault(),
            input.EndDate.GetValueOrDefault());

        return Task.FromResult(request.Id);
    }

    public Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForManagerAsync(
        Guid managerId,
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var consultantIds = _reviewScopes
            .Where(scope => scope.ManagerId == managerId)
            .Select(scope => scope.ConsultantId)
            .ToHashSet();

        return Task.FromResult(SearchForReview(request, item => consultantIds.Contains(item.ConsultantId)));
    }

    public Task<PagedResult<PendingLeaveRequestListItem>> GetPendingForAdminAsync(
        LeaveReviewSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(SearchForReview(request, _ => true));
    }

    public Task<LeaveRequestReviewDetail?> GetForReviewAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_requests.TryGetValue(leaveRequestId, out var request) || !CanReview(request.ConsultantId, reviewerManagerId, isAdministrator))
            {
                return Task.FromResult<LeaveRequestReviewDetail?>(null);
            }

            return Task.FromResult<LeaveRequestReviewDetail?>(ToReviewDetail(request));
        }
    }

    public Task<IReadOnlyList<LeaveConflict>> GetConflictsAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_requests.TryGetValue(leaveRequestId, out var current) || !CanReview(current.ConsultantId, reviewerManagerId, isAdministrator))
            {
                return Task.FromResult((IReadOnlyList<LeaveConflict>)[]);
            }

            var conflicts = _dayRows
                .Where(row => row.LeaveRequestId != leaveRequestId)
                .Where(row => row.LeaveDate >= current.StartDate && row.LeaveDate <= current.EndDate)
                .GroupBy(row => new { row.ConsultantId, row.LeaveDate })
                .Select(group => new LeaveConflict(group.Key.ConsultantId, "Approved Consultant", "approved@leaveflow.test", group.Key.LeaveDate))
                .OrderBy(row => row.ConflictDate)
                .ToArray();

            return Task.FromResult((IReadOnlyList<LeaveConflict>)conflicts);
        }
    }

    public Task<bool> ApproveAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!TryGetReviewablePending(leaveRequestId, reviewerManagerId, isAdministrator, out var request))
            {
                return Task.FromResult(false);
            }

            var approved = request with
            {
                Status = LeaveRequestStatuses.Approved,
                ReviewedAt = DateTime.UtcNow,
                ReviewedBy = reviewerUserId,
                ReviewNote = string.IsNullOrWhiteSpace(input.ReviewNote) ? null : input.ReviewNote.Trim()
            };
            _requests[leaveRequestId] = approved;
            AddDayRows(approved);
            return Task.FromResult(true);
        }
    }

    public Task<bool> RejectAsync(
        Guid leaveRequestId,
        Guid reviewerUserId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        ReviewDecisionInput input,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!TryGetReviewablePending(leaveRequestId, reviewerManagerId, isAdministrator, out var request))
            {
                return Task.FromResult(false);
            }

            _requests[leaveRequestId] = request with
            {
                Status = LeaveRequestStatuses.Rejected,
                ReviewedAt = DateTime.UtcNow,
                ReviewedBy = reviewerUserId,
                ReviewNote = string.IsNullOrWhiteSpace(input.ReviewNote) ? null : input.ReviewNote.Trim()
            };
            return Task.FromResult(true);
        }
    }

    private PagedResult<PendingLeaveRequestListItem> SearchForReview(
        LeaveReviewSearchRequest request,
        Func<LeaveRequestDetail, bool> scope)
    {
        LeaveRequestDetail[] items;
        lock (_gate)
        {
            items = _requests.Values
                .Where(scope)
                .Where(item => request.ConsultantId is null || item.ConsultantId == request.ConsultantId)
                .Where(item => request.Status is null || item.Status == request.Status)
                .Where(item => request.FromDate is null || item.EndDate >= request.FromDate)
                .Where(item => request.ToDate is null || item.StartDate <= request.ToDate)
                .OrderBy(item => item.CreatedAt)
                .ToArray();
        }

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new PendingLeaveRequestListItem(
                item.Id,
                item.ConsultantId,
                "Test Consultant",
                "consultant@leaveflow.test",
                item.Reason,
                item.StartDate,
                item.EndDate,
                item.Status,
                item.CreatedAt,
                items.Length))
            .ToArray();

        return new PagedResult<PendingLeaveRequestListItem>(page, request.PageNumber, request.PageSize, items.Length);
    }

    private bool TryGetReviewablePending(
        Guid leaveRequestId,
        Guid? reviewerManagerId,
        bool isAdministrator,
        out LeaveRequestDetail request)
    {
        if (!_requests.TryGetValue(leaveRequestId, out var found)
            || found.Status != LeaveRequestStatuses.Pending
            || !CanReview(found.ConsultantId, reviewerManagerId, isAdministrator))
        {
            request = default!;
            return false;
        }

        request = found;
        return true;
    }

    private bool CanReview(Guid consultantId, Guid? reviewerManagerId, bool isAdministrator)
    {
        return isAdministrator || (reviewerManagerId is not null && _reviewScopes.Contains((reviewerManagerId.Value, consultantId)));
    }

    private static LeaveRequestReviewDetail ToReviewDetail(LeaveRequestDetail request)
    {
        return new LeaveRequestReviewDetail(
            request.Id,
            request.ConsultantId,
            "Test Consultant",
            "consultant@leaveflow.test",
            request.Reason,
            request.StartDate,
            request.EndDate,
            request.Status,
            request.CreatedAt,
            request.ReviewedAt,
            request.ReviewedBy,
            request.ReviewNote);
    }

    private void AddDayRows(LeaveRequestDetail request)
    {
        for (var day = request.StartDate; day <= request.EndDate; day = day.AddDays(1))
        {
            _dayRows.Add((request.ConsultantId, request.Id, day));
        }
    }
}
