namespace LeaveFlow.IntegrationTests;

public sealed class LeaveRequestSqlScriptContractTests
{
    [Fact]
    public void CreateProcedure_Should_InsertOnlyLeaveRequests()
    {
        var sql = ReadStoredProcedure("043_CreateLeaveRequestsCreate.sql");

        Assert.Contains("INSERT INTO dbo.LeaveRequests", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateProcedure_Should_SetPendingStatus()
    {
        var sql = ReadStoredProcedure("043_CreateLeaveRequestsCreate.sql");

        Assert.Contains("N'Pending'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("N'Approved'", sql.Replace("Status IN (N'Pending', N'Approved')", string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OverlapProcedure_Should_CheckPendingAndApprovedOnly()
    {
        var sql = ReadStoredProcedure("042_CreateLeaveRequestsExistsOverlap.sql");

        Assert.Contains("Status IN (N'Pending', N'Approved')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Rejected", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetMineProcedure_Should_FilterByConsultantStatusAndDateRange()
    {
        var sql = ReadStoredProcedure("044_CreateLeaveRequestsGetMine.sql");

        Assert.Contains("ConsultantId = @ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@Status IS NULL OR Status = @Status", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@FromDate IS NULL OR EndDate >= @FromDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@ToDate IS NULL OR StartDate <= @ToDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetByIdProcedure_Should_ScopeByConsultant()
    {
        var sql = ReadStoredProcedure("045_CreateLeaveRequestsGetById.sql");

        Assert.Contains("Id = @LeaveRequestId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ConsultantId = @ConsultantId", sql, StringComparison.OrdinalIgnoreCase);
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
