namespace LeaveFlow.Application.Reporting;

public static class ReportingMapper
{
    public static IReadOnlyList<DashboardMetric> ToMetrics(DashboardData data, string scope)
    {
        IReadOnlyList<DashboardMetric> metrics = scope switch
        {
            "Consultant" =>
            new DashboardMetric[]
            {
                new DashboardMetric("Pending requests", data.PendingLeaveRequestCount),
                new DashboardMetric("Upcoming approved leaves", data.UpcomingLeaveCount),
                new DashboardMetric("Upcoming holidays", CountHolidaySlots(data))
            },
            "Manager" =>
            new DashboardMetric[]
            {
                new DashboardMetric("Team consultants", data.ActiveConsultantCount),
                new DashboardMetric("Team pending requests", data.PendingLeaveRequestCount),
                new DashboardMetric("Team on leave today", data.OnLeaveTodayCount),
                new DashboardMetric("Upcoming team leaves", data.UpcomingLeaveCount)
            },
            _ =>
            new DashboardMetric[]
            {
                new DashboardMetric("Active consultants", data.ActiveConsultantCount),
                new DashboardMetric("Active managers", data.ActiveManagerCount),
                new DashboardMetric("Pending requests", data.PendingLeaveRequestCount),
                new DashboardMetric("On leave today", data.OnLeaveTodayCount),
                new DashboardMetric("Upcoming leaves", data.UpcomingLeaveCount)
            }
        };

        return metrics;
    }

    public static IReadOnlyList<DashboardMetric> ToReportSummary(
        IReadOnlyList<LeaveUsageReportRow> consultantUsage,
        IReadOnlyList<MonthlyLeaveActivityRow> monthlyActivity,
        IReadOnlyList<PeakLeaveDay> peakLeaveDays,
        IReadOnlyList<StatusDistributionItem> statusDistribution)
    {
        return
        [
            new DashboardMetric("Approved leave days", consultantUsage.Sum(row => row.ApprovedLeaveDayCount)),
            new DashboardMetric("Leave requests", monthlyActivity.Sum(row => row.RequestCount)),
            new DashboardMetric("Peak day count", peakLeaveDays.FirstOrDefault()?.ConsultantCount ?? 0),
            new DashboardMetric("Pending requests", statusDistribution.FirstOrDefault(row => row.Status == "Pending")?.RequestCount ?? 0)
        ];
    }

    private static int CountHolidaySlots(DashboardData data)
    {
        var count = 0;
        if (data.NextOrganizationHolidayDate is not null)
        {
            count++;
        }

        if (data.NextOfficialHolidayDate is not null)
        {
            count++;
        }

        return count;
    }
}
