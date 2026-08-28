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
                new DashboardMetric("Bekleyen talepler", data.PendingLeaveRequestCount),
                new DashboardMetric("Yaklaşan onaylı izinler", data.UpcomingLeaveCount),
                new DashboardMetric("Yaklaşan tatiller", CountHolidaySlots(data))
            },
            "Manager" =>
            new DashboardMetric[]
            {
                new DashboardMetric("Ekip danışmanları", data.ActiveConsultantCount),
                new DashboardMetric("Bekleyen ekip talepleri", data.PendingLeaveRequestCount),
                new DashboardMetric("Bugün izindeki ekip", data.OnLeaveTodayCount),
                new DashboardMetric("Yaklaşan ekip izinleri", data.UpcomingLeaveCount)
            },
            _ =>
            new DashboardMetric[]
            {
                new DashboardMetric("Aktif danışmanlar", data.ActiveConsultantCount),
                new DashboardMetric("Aktif yöneticiler", data.ActiveManagerCount),
                new DashboardMetric("Bekleyen talepler", data.PendingLeaveRequestCount),
                new DashboardMetric("Bugün izinde", data.OnLeaveTodayCount),
                new DashboardMetric("Yaklaşan izinler", data.UpcomingLeaveCount)
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
            new DashboardMetric("Onaylı izin günleri", consultantUsage.Sum(row => row.ApprovedLeaveDayCount)),
            new DashboardMetric("İzin talepleri", monthlyActivity.Sum(row => row.RequestCount)),
            new DashboardMetric("En yoğun gün sayısı", peakLeaveDays.FirstOrDefault()?.ConsultantCount ?? 0),
            new DashboardMetric("Bekleyen talepler", statusDistribution.FirstOrDefault(row => row.Status == "Pending")?.RequestCount ?? 0)
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
