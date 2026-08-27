CREATE OR ALTER PROCEDURE dbo.usp_OfficialHolidayDefinitions_GetById
    @OfficialHolidayDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hd.Id,
        hd.Name,
        hd.StartDate,
        hd.EndDate
    FROM dbo.OfficialHolidayDefinitions AS hd
    WHERE hd.Id = @OfficialHolidayDefinitionId;

    SELECT HolidayDate
    FROM dbo.OfficialHolidayDays
    WHERE OfficialHolidayDefinitionId = @OfficialHolidayDefinitionId
    ORDER BY HolidayDate;
END;
