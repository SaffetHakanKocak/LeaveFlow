CREATE OR ALTER PROCEDURE dbo.usp_Reports_StatusDistribution
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lr.Status,
        COUNT(1) AS RequestCount
    FROM dbo.LeaveRequests AS lr
    WHERE lr.StartDate <= @EndDate
        AND lr.EndDate >= @StartDate
        AND (@ConsultantId IS NULL OR lr.ConsultantId = @ConsultantId)
        AND (@ManagerId IS NULL OR EXISTS (SELECT 1 FROM dbo.ManagerConsultants AS mc WHERE mc.ManagerId = @ManagerId AND mc.ConsultantId = lr.ConsultantId))
    GROUP BY lr.Status
    ORDER BY lr.Status ASC;
END;
