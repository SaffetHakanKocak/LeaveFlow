CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_GetById
    @HolidayDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hd.Id,
        hd.Name,
        hd.StartDate,
        hd.EndDate,
        hd.IsActive
    FROM dbo.HolidayDefinitions AS hd
    WHERE hd.Id = @HolidayDefinitionId;

    SELECT HolidayDate
    FROM dbo.HolidayDays
    WHERE HolidayDefinitionId = @HolidayDefinitionId
    ORDER BY HolidayDate;
END;
