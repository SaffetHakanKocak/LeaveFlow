CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetForAdmin
    @Today date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(1) FROM dbo.Consultants WHERE IsActive = 1) AS ActiveConsultantCount,
        (SELECT COUNT(1) FROM dbo.Managers WHERE IsActive = 1) AS ActiveManagerCount,
        (SELECT COUNT(1) FROM dbo.LeaveRequests WHERE Status = N'Pending') AS PendingLeaveRequestCount,
        (SELECT COUNT(DISTINCT ConsultantId) FROM dbo.ConsultantLeaveDays WHERE LeaveDate = @Today) AS OnLeaveTodayCount,
        (SELECT COUNT(DISTINCT LeaveRequestId) FROM dbo.ConsultantLeaveDays WHERE LeaveDate > @Today AND LeaveDate <= DATEADD(day, 60, @Today)) AS UpcomingLeaveCount,
        (SELECT TOP (1) hd.Name FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHoliday,
        (SELECT TOP (1) hd.HolidayDate FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHolidayDate,
        (SELECT TOP (1) ohd.Name FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHoliday,
        (SELECT TOP (1) ohd.HolidayDate FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHolidayDate;
END;
