using LeaveFlow.Application.Reporting;

namespace LeaveFlow.Application.Abstractions.Reporting;

public interface IReportingRepository
{
    Task<DashboardData> GetDashboardForAdminAsync(DateOnly today, CancellationToken cancellationToken = default);

    Task<DashboardData> GetDashboardForManagerAsync(Guid managerId, DateOnly today, CancellationToken cancellationToken = default);

    Task<DashboardData> GetDashboardForConsultantAsync(Guid consultantId, DateOnly today, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveUsageReportRow>> GetConsultantLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyLeaveActivityRow>> GetMonthlyLeaveActivityAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamLeaveUsageRow>> GetTeamLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PeakLeaveDay>> GetPeakLeaveDaysAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatusDistributionItem>> GetStatusDistributionAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpcomingLeaveItem>> GetUpcomingLeavesAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpcomingHolidayItem>> GetUpcomingHolidaysAsync(ReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestSnapshot>> GetRecentLeaveRequestsAsync(ReportQuery query, CancellationToken cancellationToken = default);
}
