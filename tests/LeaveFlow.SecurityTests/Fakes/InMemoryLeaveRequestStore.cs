using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Domain.LeaveRequests;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryLeaveRequestStore : ILeaveRequestRepository
{
    private readonly Dictionary<Guid, LeaveRequestDetail> _requests = new();

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

        _requests[detail.Id] = detail;
        return detail;
    }

    public Task<PagedResult<LeaveRequestListItem>> GetMineAsync(
        Guid consultantId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = _requests.Values
            .Where(item => item.ConsultantId == consultantId)
            .Where(item => request.Status is null || item.Status == request.Status)
            .Where(item => request.FromDate is null || item.EndDate >= request.FromDate)
            .Where(item => request.ToDate is null || item.StartDate <= request.ToDate)
            .OrderByDescending(item => item.CreatedAt)
            .ToArray();

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
        return Task.FromResult(
            _requests.TryGetValue(leaveRequestId, out var request) && request.ConsultantId == consultantId
                ? request
                : null);
    }

    public Task<bool> ExistsOverlapAsync(
        Guid consultantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        var exists = _requests.Values.Any(request =>
            request.ConsultantId == consultantId
            && (request.Status == LeaveRequestStatuses.Pending || request.Status == LeaveRequestStatuses.Approved)
            && request.StartDate <= endDate
            && request.EndDate >= startDate);

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
}
