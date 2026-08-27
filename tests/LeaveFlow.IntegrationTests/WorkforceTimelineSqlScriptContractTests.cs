namespace LeaveFlow.IntegrationTests;

public sealed class WorkforceTimelineSqlScriptContractTests
{
    [Fact]
    public void AdminTimeline_Should_ReadApprovedLeaveDays_FromConsultantLeaveDays()
    {
        var sql = ReadStoredProcedure("052_CreateWorkforceTimelineGetForAdmin.sql");

        Assert.Contains("dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lr.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ManagerId IS NULL OR EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ManagerTimeline_Should_ScopeAtDatabaseLevel_ByManagerAssignments()
    {
        var sql = ReadStoredProcedure("053_CreateWorkforceTimelineGetForManager.sql");

        Assert.Contains("FROM dbo.ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mc.ManagerId = @ManagerId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lr.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("052_CreateWorkforceTimelineGetForAdmin.sql")]
    [InlineData("053_CreateWorkforceTimelineGetForManager.sql")]
    public void TimelineQueries_Should_NotUseProceduralPerConsultantLoops(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.DoesNotContain("CURSOR", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WHILE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Timeline_Should_HaveDateAndConsultantIndexes()
    {
        var sql = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "db", "002_Indexes", "006_CreateWorkforceTimelineIndexes.sql"));

        Assert.Contains("IX_ConsultantLeaveDays_Date_Consultant", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LeaveDate, ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("INCLUDE (LeaveRequestId)", sql, StringComparison.OrdinalIgnoreCase);
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
