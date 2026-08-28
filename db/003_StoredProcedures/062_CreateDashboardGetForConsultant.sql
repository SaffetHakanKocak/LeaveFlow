CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetForConsultant
    @ConsultantId uniqueidentifier,
    @Today date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        0 AS ActiveConsultantCount,
        0 AS ActiveManagerCount,
        (SELECT COUNT(1) FROM dbo.LeaveRequests WHERE ConsultantId = @ConsultantId AND Status = N'Pending') AS PendingLeaveRequestCount,
        (SELECT COUNT(DISTINCT ConsultantId) FROM dbo.ConsultantLeaveDays WHERE ConsultantId = @ConsultantId AND LeaveDate = @Today) AS OnLeaveTodayCount,
        (SELECT COUNT(DISTINCT LeaveRequestId) FROM dbo.ConsultantLeaveDays WHERE ConsultantId = @ConsultantId AND LeaveDate > @Today AND LeaveDate <= DATEADD(day, 60, @Today)) AS UpcomingLeaveCount,
        (SELECT TOP (1) hd.Name FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHoliday,
        (SELECT TOP (1) hd.HolidayDate FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHolidayDate,
        (SELECT TOP (1) ohd.Name FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHoliday,
        (SELECT TOP (1) ohd.HolidayDate FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHolidayDate;
END;
