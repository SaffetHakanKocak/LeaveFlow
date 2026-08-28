CREATE OR ALTER PROCEDURE dbo.usp_Reports_ConsultantLeaveUsage
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id AS ConsultantId,
        CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
        COUNT(1) AS ApprovedLeaveDayCount
    FROM dbo.ConsultantLeaveDays AS cld
    INNER JOIN dbo.LeaveRequests AS lr ON lr.Id = cld.LeaveRequestId
    INNER JOIN dbo.Consultants AS c ON c.Id = cld.ConsultantId
    WHERE cld.LeaveDate BETWEEN @StartDate AND @EndDate
        AND lr.Status = N'Approved'
        AND (@ConsultantId IS NULL OR c.Id = @ConsultantId)
        AND (@ManagerId IS NULL OR EXISTS (SELECT 1 FROM dbo.ManagerConsultants AS mc WHERE mc.ManagerId = @ManagerId AND mc.ConsultantId = c.Id))
    GROUP BY c.Id, c.FirstName, c.LastName
    ORDER BY ApprovedLeaveDayCount DESC, ConsultantName ASC;
END;
