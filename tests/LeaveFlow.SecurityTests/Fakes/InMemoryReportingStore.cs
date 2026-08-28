using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Reporting;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryReportingStore : IReportingRepository
{
    private readonly List<ReportingConsultant> _consultants = [];
    private readonly List<ReportingLeave> _leaves = [];
    private readonly List<UpcomingHolidayItem> _holidays = [];
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _assignments = [];

    public void AddConsultant(Guid consultantId, string name)
    {
        _consultants.RemoveAll(item => item.Id == consultantId);
        _consultants.Add(new ReportingConsultant(consultantId, name));
    }

    public void AssignManager(Guid managerId, Guid consultantId)
    {
        _assignments.Add((managerId, consultantId));
    }

    public void AddLeave(Guid leaveRequestId, Guid consultantId, string status, DateOnly startDate, DateOnly endDate)
    {
        _leaves.Add(new ReportingLeave(leaveRequestId, consultantId, status, startDate, endDate));
    }

    public void AddHoliday(UpcomingHolidayItem holiday)
    {
        _holidays.Add(holiday);
    }

    public Task<DashboardData> GetDashboardForAdminAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BuildDashboard(_consultants.Select(item => item.Id).ToHashSet(), today, activeManagers: 2));
    }

    public Task<DashboardData> GetDashboardForManagerAsync(Guid managerId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var scoped = _assignments.Where(item => item.ManagerId == managerId).Select(item => item.ConsultantId).ToHashSet();
        return Task.FromResult(BuildDashboard(scoped, today, activeManagers: 0));
    }

    public Task<DashboardData> GetDashboardForConsultantAsync(Guid consultantId, DateOnly today, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BuildDashboard([consultantId], today, activeManagers: 0));
    }

    public Task<IReadOnlyList<LeaveUsageReportRow>> GetConsultantLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<LeaveUsageReportRow>>(ApprovedLeaves(query)
            .GroupBy(item => item.ConsultantId)
            .Select(group =>
            {
                var consultant = _consultants.Single(item => item.Id == group.Key);
                return new LeaveUsageReportRow(group.Key, consultant.Name, group.Sum(item => DayCount(item.StartDate, item.EndDate)));
            })
            .ToArray());
    }

    public Task<IReadOnlyList<MonthlyLeaveActivityRow>> GetMonthlyLeaveActivityAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MonthlyLeaveActivityRow>>(ApprovedLeaves(query)
            .SelectMany(leave => Days(leave.StartDate, leave.EndDate).Select(day => new { Leave = leave, Day = day }))
            .GroupBy(item => new { item.Day.Year, item.Day.Month })
            .Select(group => new MonthlyLeaveActivityRow(group.Key.Year, group.Key.Month, group.Count(), group.Select(item => item.Leave.LeaveRequestId).Distinct().Count()))
            .ToArray());
    }

    public Task<IReadOnlyList<TeamLeaveUsageRow>> GetTeamLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        var managerIds = query.ManagerId is null ? _assignments.Select(item => item.ManagerId).Distinct() : [query.ManagerId.Value];
        return Task.FromResult<IReadOnlyList<TeamLeaveUsageRow>>(managerIds
            .Select(managerId =>
            {
                var consultantIds = _assignments.Where(item => item.ManagerId == managerId).Select(item => item.ConsultantId).ToHashSet();
                var days = ApprovedLeaves(query with { ManagerId = managerId, ConsultantId = null }).Sum(item => DayCount(item.StartDate, item.EndDate));
                return new TeamLeaveUsageRow(managerId, managerId.ToString("N")[..8], consultantIds.Count, days);
            })
            .ToArray());
    }

    public Task<IReadOnlyList<PeakLeaveDay>> GetPeakLeaveDaysAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<PeakLeaveDay>>(ApprovedLeaves(query)
            .SelectMany(leave => Days(leave.StartDate, leave.EndDate).Select(day => new { leave.ConsultantId, Day = day }))
            .GroupBy(item => item.Day)
            .Select(group => new PeakLeaveDay(group.Key, group.Select(item => item.ConsultantId).Distinct().Count()))
            .OrderByDescending(item => item.ConsultantCount)
            .Take(10)
            .ToArray());
    }

    public Task<IReadOnlyList<StatusDistributionItem>> GetStatusDistributionAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StatusDistributionItem>>(ScopedLeaves(query)
            .GroupBy(item => item.Status)
            .Select(group => new StatusDistributionItem(group.Key, group.Count()))
            .ToArray());
    }

    public Task<IReadOnlyList<UpcomingLeaveItem>> GetUpcomingLeavesAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<UpcomingLeaveItem>>(ApprovedLeaves(query)
            .OrderBy(item => item.StartDate)
            .Take(10)
            .Select(leave =>
            {
                var consultant = _consultants.Single(item => item.Id == leave.ConsultantId);
                return new UpcomingLeaveItem(
                    leave.LeaveRequestId,
                    leave.ConsultantId,
                    query.ConsultantId is null ? consultant.Name : null,
                    leave.StartDate,
                    leave.EndDate,
                    DayCount(leave.StartDate, leave.EndDate));
            })
            .ToArray());
    }

    public Task<IReadOnlyList<UpcomingHolidayItem>> GetUpcomingHolidaysAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<UpcomingHolidayItem>>(_holidays
            .Where(item => item.StartDate <= query.EndDate && item.EndDate >= query.StartDate)
            .OrderBy(item => item.StartDate)
            .Take(10)
            .ToArray());
    }

    public Task<IReadOnlyList<LeaveRequestSnapshot>> GetRecentLeaveRequestsAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<LeaveRequestSnapshot>>(ScopedLeaves(query)
            .OrderByDescending(item => item.StartDate)
            .Take(10)
            .Select(item => new LeaveRequestSnapshot(item.LeaveRequestId, item.Status, item.StartDate, item.EndDate))
            .ToArray());
    }

    private DashboardData BuildDashboard(HashSet<Guid> consultantIds, DateOnly today, int activeManagers)
    {
        var scoped = _leaves.Where(item => consultantIds.Contains(item.ConsultantId)).ToArray();
        var nextOrg = _holidays.FirstOrDefault(item => item.EventType == "OrganizationHoliday" && item.StartDate >= today);
        var nextOfficial = _holidays.FirstOrDefault(item => item.EventType == "OfficialHoliday" && item.StartDate >= today);
        return new DashboardData(
            consultantIds.Count,
            activeManagers,
            scoped.Count(item => item.Status == "Pending"),
            scoped.Where(item => item.Status == "Approved" && item.StartDate <= today && item.EndDate >= today).Select(item => item.ConsultantId).Distinct().Count(),
            scoped.Count(item => item.Status == "Approved" && item.EndDate > today),
            nextOrg?.Name,
            nextOrg?.StartDate,
            nextOfficial?.Name,
            nextOfficial?.StartDate);
    }

    private IEnumerable<ReportingLeave> ApprovedLeaves(ReportQuery query)
    {
        return ScopedLeaves(query).Where(item => item.Status == "Approved");
    }

    private IEnumerable<ReportingLeave> ScopedLeaves(ReportQuery query)
    {
        return _leaves
            .Where(item => item.StartDate <= query.EndDate && item.EndDate >= query.StartDate)
            .Where(item => query.ConsultantId is null || item.ConsultantId == query.ConsultantId)
            .Where(item => query.ManagerId is null || _assignments.Contains((query.ManagerId.Value, item.ConsultantId)));
    }

    private static int DayCount(DateOnly startDate, DateOnly endDate) => endDate.DayNumber - startDate.DayNumber + 1;

    private static IEnumerable<DateOnly> Days(DateOnly startDate, DateOnly endDate)
    {
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private sealed record ReportingConsultant(Guid Id, string Name);

    private sealed record ReportingLeave(Guid LeaveRequestId, Guid ConsultantId, string Status, DateOnly StartDate, DateOnly EndDate);
}
