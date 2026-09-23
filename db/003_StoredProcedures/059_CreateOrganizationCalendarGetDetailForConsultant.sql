CREATE OR ALTER PROCEDURE dbo.usp_OrganizationCalendar_GetDetailForConsultant
    @ConsultantId uniqueidentifier,
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
            N'İznim' AS Title,
            lr.StartDate,
            lr.EndDate,
            lr.ConsultantId,
            CAST(NULL AS nvarchar(161)) AS ConsultantName,
            lr.Id AS LeaveRequestId,
            CAST(NULL AS uniqueidentifier) AS HolidayDefinitionId,
            lr.Reason AS Summary
        FROM dbo.LeaveRequests AS lr
        WHERE lr.Id = @EventId
            AND lr.ConsultantId = @ConsultantId
            AND lr.Status = N'Approved';
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
