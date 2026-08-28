CREATE OR ALTER PROCEDURE dbo.usp_Reports_MonthlyLeaveActivity
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        YEAR(cld.LeaveDate) AS [Year],
        MONTH(cld.LeaveDate) AS [Month],
        COUNT(1) AS ApprovedLeaveDayCount,
        COUNT(DISTINCT cld.LeaveRequestId) AS RequestCount
    FROM dbo.ConsultantLeaveDays AS cld
    INNER JOIN dbo.LeaveRequests AS lr ON lr.Id = cld.LeaveRequestId
    WHERE cld.LeaveDate BETWEEN @StartDate AND @EndDate
        AND lr.Status = N'Approved'
        AND (@ConsultantId IS NULL OR lr.ConsultantId = @ConsultantId)
        AND (@ManagerId IS NULL OR EXISTS (SELECT 1 FROM dbo.ManagerConsultants AS mc WHERE mc.ManagerId = @ManagerId AND mc.ConsultantId = lr.ConsultantId))
    GROUP BY YEAR(cld.LeaveDate), MONTH(cld.LeaveDate)
    ORDER BY [Year] ASC, [Month] ASC;
END;
