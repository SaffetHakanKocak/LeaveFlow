namespace LeaveFlow.Application.Reporting;

public sealed record ReportQuery(
    DateOnly? StartDate,
    DateOnly? EndDate,
    Guid? ManagerId,
    Guid? ConsultantId);

public sealed record DashboardMetric(string Label, int Value);

public sealed record DashboardData(
    int ActiveConsultantCount,
    int ActiveManagerCount,
    int PendingLeaveRequestCount,
    int OnLeaveTodayCount,
    int UpcomingLeaveCount,
    string? NextOrganizationHoliday,
    DateOnly? NextOrganizationHolidayDate,
    string? NextOfficialHoliday,
    DateOnly? NextOfficialHolidayDate);

public sealed record DashboardSummary(
    string Scope,
    IReadOnlyList<DashboardMetric> Metrics,
    IReadOnlyList<UpcomingLeaveItem> UpcomingLeaves,
    IReadOnlyList<UpcomingHolidayItem> UpcomingHolidays,
    IReadOnlyList<LeaveRequestSnapshot> RecentLeaveRequests);

public sealed record LeaveUsageReportRow(Guid ConsultantId, string ConsultantName, int ApprovedLeaveDayCount);

public sealed record MonthlyLeaveActivityRow(int Year, int Month, int ApprovedLeaveDayCount, int RequestCount);

public sealed record TeamLeaveUsageRow(Guid ManagerId, string ManagerName, int ConsultantCount, int ApprovedLeaveDayCount);

public sealed record PeakLeaveDay(DateOnly LeaveDate, int ConsultantCount);

public sealed record StatusDistributionItem(string Status, int RequestCount);

public sealed record UpcomingLeaveItem(Guid LeaveRequestId, Guid ConsultantId, string? ConsultantName, DateOnly StartDate, DateOnly EndDate, int DayCount);

public sealed record UpcomingHolidayItem(string EventType, Guid HolidayDefinitionId, string Name, DateOnly StartDate, DateOnly EndDate);

public sealed record LeaveRequestSnapshot(Guid LeaveRequestId, string Status, DateOnly StartDate, DateOnly EndDate);

public sealed record ReportsResult(
    ReportQuery Query,
    IReadOnlyList<DashboardMetric> SummaryMetrics,
    IReadOnlyList<LeaveUsageReportRow> ConsultantLeaveUsage,
    IReadOnlyList<MonthlyLeaveActivityRow> MonthlyLeaveActivity,
    IReadOnlyList<TeamLeaveUsageRow> TeamLeaveUsage,
    IReadOnlyList<PeakLeaveDay> PeakLeaveDays,
    IReadOnlyList<StatusDistributionItem> StatusDistribution,
    IReadOnlyList<UpcomingLeaveItem> UpcomingLeaves,
    IReadOnlyList<UpcomingHolidayItem> UpcomingHolidays,
    int MaxRangeDays);
