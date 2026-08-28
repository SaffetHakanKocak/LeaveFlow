CREATE OR ALTER PROCEDURE dbo.usp_OrganizationCalendar_GetDetailForManager
    @ManagerId uniqueidentifier,
    @EventType nvarchar(64),
    @EventId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF @EventType = N'Leave'
    BEGIN
        SELECT
            N'Leave' AS EventType,
            lr.Id AS EventId,
            CONCAT(c.FirstName, N' ', c.LastName, N' leave') AS Title,
            lr.StartDate,
            lr.EndDate,
            c.Id AS ConsultantId,
            CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
            lr.Id AS LeaveRequestId,
            CAST(NULL AS uniqueidentifier) AS HolidayDefinitionId,
            lr.Reason AS Summary
        FROM dbo.LeaveRequests AS lr
        INNER JOIN dbo.Consultants AS c ON c.Id = lr.ConsultantId
        WHERE lr.Id = @EventId
            AND lr.Status = N'Approved'
            AND EXISTS
            (
                SELECT 1
                FROM dbo.ManagerConsultants AS mc
                WHERE mc.ManagerId = @ManagerId
                    AND mc.ConsultantId = lr.ConsultantId
            );
        RETURN;
    END;

    IF @EventType = N'OrganizationHoliday'
    BEGIN
        SELECT
            N'OrganizationHoliday' AS EventType,
            h.Id AS EventId,
            h.Name AS Title,
            h.StartDate,
            h.EndDate,
            CAST(NULL AS uniqueidentifier) AS ConsultantId,
            CAST(NULL AS nvarchar(161)) AS ConsultantName,
            CAST(NULL AS uniqueidentifier) AS LeaveRequestId,
            h.Id AS HolidayDefinitionId,
            h.Description AS Summary
        FROM dbo.HolidayDefinitions AS h
        WHERE h.Id = @EventId
            AND h.IsActive = 1;
        RETURN;
    END;

    IF @EventType = N'OfficialHoliday'
    BEGIN
        SELECT
            N'OfficialHoliday' AS EventType,
            h.Id AS EventId,
            h.Name AS Title,
            h.StartDate,
            h.EndDate,
            CAST(NULL AS uniqueidentifier) AS ConsultantId,
            CAST(NULL AS nvarchar(161)) AS ConsultantName,
            CAST(NULL AS uniqueidentifier) AS LeaveRequestId,
            h.Id AS HolidayDefinitionId,
            CONCAT(h.CountryCode, COALESCE(N'/' + h.RegionCode, N'')) AS Summary
        FROM dbo.OfficialHolidayDefinitions AS h
        WHERE h.Id = @EventId
            AND h.IsActive = 1;
    END;
END;
