CREATE OR ALTER PROCEDURE dbo.usp_Reports_TeamLeaveUsage
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.Id AS ManagerId,
        CONCAT(m.FirstName, N' ', m.LastName) AS ManagerName,
        COUNT(DISTINCT mc.ConsultantId) AS ConsultantCount,
        COUNT(cld.Id) AS ApprovedLeaveDayCount
    FROM dbo.Managers AS m
    INNER JOIN dbo.ManagerConsultants AS mc ON mc.ManagerId = m.Id
    LEFT JOIN dbo.ConsultantLeaveDays AS cld
        ON cld.ConsultantId = mc.ConsultantId
        AND cld.LeaveDate BETWEEN @StartDate AND @EndDate
    LEFT JOIN dbo.LeaveRequests AS lr
        ON lr.Id = cld.LeaveRequestId
        AND lr.Status = N'Approved'
    WHERE (@ManagerId IS NULL OR m.Id = @ManagerId)
        AND (@ConsultantId IS NULL OR mc.ConsultantId = @ConsultantId)
        AND (cld.Id IS NULL OR lr.Id IS NOT NULL)
    GROUP BY m.Id, m.FirstName, m.LastName
    ORDER BY ApprovedLeaveDayCount DESC, ManagerName ASC;
END;
