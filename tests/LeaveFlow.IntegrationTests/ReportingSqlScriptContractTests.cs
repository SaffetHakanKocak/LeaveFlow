namespace LeaveFlow.IntegrationTests;

public sealed class ReportingSqlScriptContractTests
{
    [Theory]
    [InlineData("060_CreateDashboardGetForAdmin.sql")]
    [InlineData("061_CreateDashboardGetForManager.sql")]
    [InlineData("062_CreateDashboardGetForConsultant.sql")]
    public void DashboardProcedures_Should_ReturnExpectedMetricShape(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("ActiveConsultantCount", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PendingLeaveRequestCount", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OnLeaveTodayCount", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UpcomingLeaveCount", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("063_CreateReportsConsultantLeaveUsage.sql")]
    [InlineData("064_CreateReportsMonthlyLeaveActivity.sql")]
    [InlineData("066_CreateReportsPeakLeaveDays.sql")]
    public void ApprovedLeaveAnalytics_Should_ReadConsultantLeaveDays_AndApprovedRequests(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lr.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ManagerId IS NULL OR EXISTS", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StatusDistribution_Should_ReadLeaveRequests_ForWorkflowAnalytics()
    {
        var sql = ReadStoredProcedure("067_CreateReportsStatusDistribution.sql");

        Assert.Contains("dbo.LeaveRequests", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GROUP BY lr.Status", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RecentLeaveRequests_Should_ReadLeaveRequests_ForWorkflowAnalytics()
    {
        var sql = ReadStoredProcedure("070_CreateReportsRecentLeaveRequests.sql");

        Assert.Contains("dbo.LeaveRequests", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lr.Status", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ORDER BY lr.CreatedAt DESC", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpcomingHolidays_Should_ReadOrganizationAndOfficialHolidayDefinitions()
    {
        var sql = ReadStoredProcedure("069_CreateReportsUpcomingHolidays.sql");

        Assert.Contains("dbo.HolidayDefinitions", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.OfficialHolidayDefinitions", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("h.IsActive = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("oh.IsActive = 1", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("063_CreateReportsConsultantLeaveUsage.sql")]
    [InlineData("064_CreateReportsMonthlyLeaveActivity.sql")]
    [InlineData("065_CreateReportsTeamLeaveUsage.sql")]
    [InlineData("066_CreateReportsPeakLeaveDays.sql")]
    [InlineData("067_CreateReportsStatusDistribution.sql")]
    [InlineData("068_CreateReportsUpcomingLeaves.sql")]
    [InlineData("070_CreateReportsRecentLeaveRequests.sql")]
    public void ReportProcedures_Should_SupportDateRangeAndScopeParameters(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("@StartDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@EndDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ManagerId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("063_CreateReportsConsultantLeaveUsage.sql")]
    [InlineData("064_CreateReportsMonthlyLeaveActivity.sql")]
    [InlineData("065_CreateReportsTeamLeaveUsage.sql")]
    [InlineData("066_CreateReportsPeakLeaveDays.sql")]
    [InlineData("067_CreateReportsStatusDistribution.sql")]
    [InlineData("068_CreateReportsUpcomingLeaves.sql")]
    [InlineData("069_CreateReportsUpcomingHolidays.sql")]
    [InlineData("070_CreateReportsRecentLeaveRequests.sql")]
    public void ReportProcedures_Should_NotUseProceduralNPlusOneLoops(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.DoesNotContain("CURSOR", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WHILE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportingIndexes_Should_SupportStatusAndDateLookups()
    {
        var sql = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "db", "002_Indexes", "008_CreateReportingIndexes.sql"));

        Assert.Contains("IX_LeaveRequests_Status_Date_Consultant", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Status, StartDate, EndDate, ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadStoredProcedure(string fileName)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), "db", "003_StoredProcedures", fileName));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "db")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
