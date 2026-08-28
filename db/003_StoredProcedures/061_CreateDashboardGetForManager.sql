CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetForManager
    @ManagerId uniqueidentifier,
    @Today date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(1) FROM dbo.ManagerConsultants AS mc INNER JOIN dbo.Consultants AS c ON c.Id = mc.ConsultantId WHERE mc.ManagerId = @ManagerId AND c.IsActive = 1) AS ActiveConsultantCount,
        0 AS ActiveManagerCount,
        (SELECT COUNT(1) FROM dbo.LeaveRequests AS lr INNER JOIN dbo.ManagerConsultants AS mc ON mc.ConsultantId = lr.ConsultantId WHERE mc.ManagerId = @ManagerId AND lr.Status = N'Pending') AS PendingLeaveRequestCount,
        (SELECT COUNT(DISTINCT cld.ConsultantId) FROM dbo.ConsultantLeaveDays AS cld INNER JOIN dbo.ManagerConsultants AS mc ON mc.ConsultantId = cld.ConsultantId WHERE mc.ManagerId = @ManagerId AND cld.LeaveDate = @Today) AS OnLeaveTodayCount,
        (SELECT COUNT(DISTINCT cld.LeaveRequestId) FROM dbo.ConsultantLeaveDays AS cld INNER JOIN dbo.ManagerConsultants AS mc ON mc.ConsultantId = cld.ConsultantId WHERE mc.ManagerId = @ManagerId AND cld.LeaveDate > @Today AND cld.LeaveDate <= DATEADD(day, 60, @Today)) AS UpcomingLeaveCount,
        (SELECT TOP (1) hd.Name FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHoliday,
        (SELECT TOP (1) hd.HolidayDate FROM dbo.HolidayDays AS hd INNER JOIN dbo.HolidayDefinitions AS h ON h.Id = hd.HolidayDefinitionId WHERE h.IsActive = 1 AND hd.HolidayDate >= @Today ORDER BY hd.HolidayDate ASC) AS NextOrganizationHolidayDate,
        (SELECT TOP (1) ohd.Name FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHoliday,
        (SELECT TOP (1) ohd.HolidayDate FROM dbo.OfficialHolidayDays AS ohd INNER JOIN dbo.OfficialHolidayDefinitions AS oh ON oh.Id = ohd.OfficialHolidayDefinitionId WHERE oh.IsActive = 1 AND ohd.HolidayDate >= @Today ORDER BY ohd.HolidayDate ASC) AS NextOfficialHolidayDate;
END;
