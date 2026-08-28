public sealed class OrganizationCalendarSqlScriptContractTests
{
    [Theory]
    [InlineData("054_CreateOrganizationCalendarGetForAdmin.sql")]
    [InlineData("055_CreateOrganizationCalendarGetForManager.sql")]
    [InlineData("056_CreateOrganizationCalendarGetForConsultant.sql")]
    public void CalendarListProcedures_Should_CombineThreeSources_AndApprovedLeavesOnly(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.HolidayDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.OfficialHolidayDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lr.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("h.IsActive = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("oh.IsActive = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNION ALL", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ManagerCalendar_Should_ScopeLeaveEvents_ByManagerAssignments()
    {
        var sql = ReadStoredProcedure("055_CreateOrganizationCalendarGetForManager.sql");

        Assert.Contains("FROM dbo.ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mc.ManagerId = @ManagerId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ConsultantId IS NULL OR c.Id = @ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultantCalendar_Should_OnlyReadOwnLeaveEvents_AndNotReturnOtherNames()
    {
        var sql = ReadStoredProcedure("056_CreateOrganizationCalendarGetForConsultant.sql");

        Assert.Contains("cld.ConsultantId = @ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("N'My leave' AS Title", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CAST(NULL AS nvarchar(161)) AS ConsultantName", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("057_CreateOrganizationCalendarGetDetailForAdmin.sql")]
    [InlineData("058_CreateOrganizationCalendarGetDetailForManager.sql")]
    [InlineData("059_CreateOrganizationCalendarGetDetailForConsultant.sql")]
    public void CalendarDetailProcedures_Should_ProtectApprovedLeaveAndActiveHolidays(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("lr.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("h.IsActive = 1", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ManagerCalendarDetail_Should_PreventOtherTeamLeaveIdor()
    {
        var sql = ReadStoredProcedure("058_CreateOrganizationCalendarGetDetailForManager.sql");

        Assert.Contains("EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mc.ManagerId = @ManagerId", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultantCalendarDetail_Should_PreventCrossConsultantLeaveIdor()
    {
        var sql = ReadStoredProcedure("059_CreateOrganizationCalendarGetDetailForConsultant.sql");

        Assert.Contains("lr.ConsultantId = @ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CAST(NULL AS nvarchar(161)) AS ConsultantName", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalendarIndexes_Should_SupportHolidayDateRangeLookups()
    {
        var sql = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "db", "002_Indexes", "007_CreateOrganizationCalendarIndexes.sql"));

        Assert.Contains("IX_HolidayDays_Date_Definition", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HolidayDate, HolidayDefinitionId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IX_OfficialHolidayDays_Date_Definition", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadStoredProcedure(string fileName)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(root, "db", "003_StoredProcedures", fileName));
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
