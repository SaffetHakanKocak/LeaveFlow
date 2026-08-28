CREATE OR ALTER PROCEDURE dbo.usp_Reports_UpcomingHolidays
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH HolidayEvents AS
    (
        SELECT
            N'OrganizationHoliday' AS EventType,
            h.Id AS HolidayDefinitionId,
            h.Name,
            h.StartDate,
            h.EndDate
        FROM dbo.HolidayDefinitions AS h
        WHERE h.IsActive = 1
            AND h.EndDate >= @StartDate
            AND h.StartDate <= @EndDate

        UNION ALL

        SELECT
            N'OfficialHoliday' AS EventType,
            oh.Id AS HolidayDefinitionId,
            oh.Name,
            oh.StartDate,
            oh.EndDate
        FROM dbo.OfficialHolidayDefinitions AS oh
        WHERE oh.IsActive = 1
            AND oh.EndDate >= @StartDate
            AND oh.StartDate <= @EndDate
    )
    SELECT TOP (10)
        EventType,
        HolidayDefinitionId,
        Name,
        StartDate,
        EndDate
    FROM HolidayEvents
    ORDER BY StartDate ASC, Name ASC;
END;
