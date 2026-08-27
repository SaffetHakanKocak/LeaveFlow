CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_Delete
    @HolidayDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.HolidayDefinitions WHERE Id = @HolidayDefinitionId)
    BEGIN
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    DELETE FROM dbo.HolidayDays
    WHERE HolidayDefinitionId = @HolidayDefinitionId;

    DELETE FROM dbo.HolidayDefinitions
    WHERE Id = @HolidayDefinitionId;

    SELECT CAST(1 AS bit);
END;
