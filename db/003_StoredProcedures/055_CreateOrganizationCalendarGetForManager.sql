CREATE OR ALTER PROCEDURE dbo.usp_OrganizationCalendar_GetForManager
    @ManagerId uniqueidentifier,
    @StartDate date,
    @EndDate date,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        N'Leave' AS EventType,
        cld.LeaveRequestId AS EventId,
        CONCAT(c.FirstName, N' ', c.LastName, N' leave') AS Title,
        cld.LeaveDate AS EventDate,
        c.Id AS ConsultantId,
        CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
        cld.LeaveRequestId,
        CAST(NULL AS uniqueidentifier) AS HolidayDefinitionId
    FROM dbo.ManagerConsultants AS mc
    INNER JOIN dbo.ConsultantLeaveDays AS cld ON cld.ConsultantId = mc.ConsultantId
    INNER JOIN dbo.LeaveRequests AS lr ON lr.Id = cld.LeaveRequestId
    INNER JOIN dbo.Consultants AS c ON c.Id = cld.ConsultantId
    WHERE mc.ManagerId = @ManagerId
        AND cld.LeaveDate BETWEEN @StartDate AND @EndDate
        AND lr.Status = N'Approved'
        AND (@ConsultantId IS NULL OR c.Id = @ConsultantId)

    UNION ALL

    SELECT
        N'OrganizationHoliday' AS EventType,
        hd.HolidayDefinitionId AS EventId,
        hd.Name AS Title,
        hd.HolidayDate AS EventDate,
        CAST(NULL AS uniqueidentifier) AS ConsultantId,
        CAST(NULL AS nvarchar(161)) AS ConsultantName,
        CAST(NULL AS uniqueidentifier) AS LeaveRequestId,
        hd.HolidayDefinitionId
    FROM dbo.HolidayDays AS hd
    INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId
    WHERE hd.HolidayDate BETWEEN @StartDate AND @EndDate
        AND h.IsActive = 1

    UNION ALL

    SELECT
        N'OfficialHoliday' AS EventType,
        ohd.OfficialHolidayDefinitionId AS EventId,
        ohd.Name AS Title,
        ohd.HolidayDate AS EventDate,
        CAST(NULL AS uniqueidentifier) AS ConsultantId,
        CAST(NULL AS nvarchar(161)) AS ConsultantName,
        CAST(NULL AS uniqueidentifier) AS LeaveRequestId,
        ohd.OfficialHolidayDefinitionId AS HolidayDefinitionId
    FROM dbo.OfficialHolidayDays AS ohd
    INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId
    WHERE ohd.HolidayDate BETWEEN @StartDate AND @EndDate
        AND oh.IsActive = 1
    ORDER BY EventDate ASC, EventType ASC, Title ASC;
END;
