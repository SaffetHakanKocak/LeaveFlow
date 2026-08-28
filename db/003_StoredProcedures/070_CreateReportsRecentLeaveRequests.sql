CREATE OR ALTER PROCEDURE dbo.usp_Reports_RecentLeaveRequests
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (10)
        lr.Id AS LeaveRequestId,
        lr.Status,
        lr.StartDate,
        lr.EndDate
    FROM dbo.LeaveRequests AS lr
    WHERE lr.StartDate <= @EndDate
        AND lr.EndDate >= @StartDate
        AND (@ConsultantId IS NULL OR lr.ConsultantId = @ConsultantId)
        AND (
            @ManagerId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM dbo.ManagerConsultants AS mc
                WHERE mc.ManagerId = @ManagerId
                    AND mc.ConsultantId = lr.ConsultantId
            )
        )
    ORDER BY lr.CreatedAt DESC, lr.StartDate DESC;
END;
