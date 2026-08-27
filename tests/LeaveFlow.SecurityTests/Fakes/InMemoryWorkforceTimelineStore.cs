using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Timeline;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryWorkforceTimelineStore : IWorkforceTimelineRepository
{
    private readonly List<TimelineConsultant> _consultants = [];
    private readonly List<TimelineLeaveDay> _leaveDays = [];
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _assignments = [];

    public void AddConsultant(Guid consultantId, string name, string email, bool isActive = true)
    {
        _consultants.RemoveAll(consultant => consultant.Id == consultantId);
        _consultants.Add(new TimelineConsultant(consultantId, name, email, isActive));
    }

    public void AssignManager(Guid managerId, Guid consultantId)
    {
        _assignments.Add((managerId, consultantId));
    }

    public void AddApprovedLeave(Guid consultantId, string reason, DateOnly startDate, DateOnly endDate)
    {
        var leaveRequestId = Guid.NewGuid();
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            _leaveDays.Add(new TimelineLeaveDay(consultantId, leaveRequestId, day, reason));
        }
    }

    public Task<PagedResult<WorkforceTimelineDataRow>> GetForAdminAsync(
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Search(query, consultant => query.ManagerId is null
            || _assignments.Contains((query.ManagerId.Value, consultant.Id))));
    }

    public Task<PagedResult<WorkforceTimelineDataRow>> GetForManagerAsync(
        Guid managerId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Search(query, consultant => _assignments.Contains((managerId, consultant.Id))));
    }

    private PagedResult<WorkforceTimelineDataRow> Search(
        WorkforceTimelineQuery query,
        Func<TimelineConsultant, bool> scope)
    {
        var consultants = _consultants
            .Where(scope)
            .Where(consultant => query.IncludeInactive || consultant.IsActive)
            .Where(consultant => string.IsNullOrWhiteSpace(query.ConsultantSearch)
                || consultant.Name.Contains(query.ConsultantSearch, StringComparison.OrdinalIgnoreCase)
                || consultant.Email.Contains(query.ConsultantSearch, StringComparison.OrdinalIgnoreCase))
            .OrderBy(consultant => consultant.Name)
            .ToArray();

        var totalCount = consultants.Length;
        var paged = consultants
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

        var rows = new List<WorkforceTimelineDataRow>();
        foreach (var consultant in paged)
        {
            var leaveDays = _leaveDays
                .Where(leave => leave.ConsultantId == consultant.Id)
                .Where(leave => leave.LeaveDate >= query.StartDate && leave.LeaveDate <= query.EndDate)
                .OrderBy(leave => leave.LeaveDate)
                .ToArray();

            if (leaveDays.Length == 0)
            {
                rows.Add(ToRow(consultant, null, totalCount));
                continue;
            }

            rows.AddRange(leaveDays.Select(leave => ToRow(consultant, leave, totalCount)));
        }

        return new PagedResult<WorkforceTimelineDataRow>(rows, query.PageNumber, query.PageSize, totalCount);
    }

    private static WorkforceTimelineDataRow ToRow(
        TimelineConsultant consultant,
        TimelineLeaveDay? leave,
        int totalCount)
    {
        return new WorkforceTimelineDataRow(
            consultant.Id,
            consultant.Name,
            consultant.Email,
            consultant.IsActive,
            leave?.LeaveDate,
            leave?.LeaveRequestId,
            leave?.Reason,
            totalCount);
    }

    private sealed record TimelineConsultant(Guid Id, string Name, string Email, bool IsActive);

    private sealed record TimelineLeaveDay(Guid ConsultantId, Guid LeaveRequestId, DateOnly LeaveDate, string Reason);
}
