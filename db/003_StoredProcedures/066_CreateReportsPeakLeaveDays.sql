CREATE OR ALTER PROCEDURE dbo.usp_Reports_PeakLeaveDays
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (10)
        cld.LeaveDate,
        COUNT(DISTINCT cld.ConsultantId) AS ConsultantCount
    FROM dbo.ConsultantLeaveDays AS cld
    INNER JOIN dbo.LeaveRequests AS lr ON lr.Id = cld.LeaveRequestId
    WHERE cld.LeaveDate BETWEEN @StartDate AND @EndDate
        AND lr.Status = N'Approved'
        AND (@ConsultantId IS NULL OR cld.ConsultantId = @ConsultantId)
        AND (@ManagerId IS NULL OR EXISTS (SELECT 1 FROM dbo.ManagerConsultants AS mc WHERE mc.ManagerId = @ManagerId AND mc.ConsultantId = cld.ConsultantId))
    GROUP BY cld.LeaveDate
    ORDER BY ConsultantCount DESC, cld.LeaveDate ASC;
END;
