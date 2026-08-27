namespace LeaveFlow.IntegrationTests;

public sealed class LeaveApprovalSqlScriptContractTests
{
    [Fact]
    public void ManagerPendingList_Should_ScopeByManagerAssignments()
    {
        var sql = ReadStoredProcedure("046_CreateLeaveRequestsGetPendingForManager.sql");

        Assert.Contains("INNER JOIN dbo.ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mc.ManagerId = @ManagerId", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AdminPendingList_Should_NotRequireManagerAssignment()
    {
        var sql = ReadStoredProcedure("047_CreateLeaveRequestsGetPendingForAdmin.sql");

        Assert.DoesNotContain("ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@Status IS NULL OR lr.Status = @Status", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReviewDetail_Should_EnforceReviewerScope()
    {
        var sql = ReadStoredProcedure("048_CreateLeaveRequestsGetForReview.sql");

        Assert.Contains("@IsAdministrator = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dbo.ManagerConsultants", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Conflicts_Should_ReadApprovedLeaveDays_AndExcludeCurrentRequest()
    {
        var sql = ReadStoredProcedure("049_CreateLeaveRequestsGetConflicts.sql");

        Assert.Contains("dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("existing.Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("existing.Id <> @LeaveRequestId", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Approve_Should_BeTransactionalAndGenerateLeaveDays()
    {
        var sql = ReadStoredProcedure("050_CreateLeaveRequestsApprove.sql");

        Assert.Contains("BEGIN TRANSACTION", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WITH (UPDLOCK, ROWLOCK)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Status = N'Approved'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("INSERT INTO dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("COMMIT TRANSACTION", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reject_Should_BeTransactionalAndNotGenerateLeaveDays()
    {
        var sql = ReadStoredProcedure("051_CreateLeaveRequestsReject.sql");

        Assert.Contains("BEGIN TRANSACTION", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Status = N'Rejected'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO dbo.ConsultantLeaveDays", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LeaveDays_Should_HaveDuplicatePreventionIndex()
    {
        var sql = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "db", "002_Indexes", "005_CreateLeaveApprovalIndexes.sql"));

        Assert.Contains("CREATE UNIQUE INDEX UX_ConsultantLeaveDays_Consultant_Request_Date", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ConsultantId, LeaveRequestId, LeaveDate", sql, StringComparison.OrdinalIgnoreCase);
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
