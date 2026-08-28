CREATE OR ALTER PROCEDURE dbo.usp_Reports_UpcomingLeaves
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (10)
        lr.Id AS LeaveRequestId,
        c.Id AS ConsultantId,
        CASE WHEN @ConsultantId IS NULL THEN CONCAT(c.FirstName, N' ', c.LastName) ELSE CAST(NULL AS nvarchar(161)) END AS ConsultantName,
        lr.StartDate,
        lr.EndDate,
        DATEDIFF(day, lr.StartDate, lr.EndDate) + 1 AS DayCount
    FROM dbo.LeaveRequests AS lr
    INNER JOIN dbo.Consultants AS c ON c.Id = lr.ConsultantId
    WHERE lr.Status = N'Approved'
        AND lr.EndDate >= @StartDate
        AND lr.StartDate <= @EndDate
        AND (@ConsultantId IS NULL OR lr.ConsultantId = @ConsultantId)
        AND (@ManagerId IS NULL OR EXISTS (SELECT 1 FROM dbo.ManagerConsultants AS mc WHERE mc.ManagerId = @ManagerId AND mc.ConsultantId = lr.ConsultantId))
    ORDER BY lr.StartDate ASC, lr.EndDate ASC;
END;
