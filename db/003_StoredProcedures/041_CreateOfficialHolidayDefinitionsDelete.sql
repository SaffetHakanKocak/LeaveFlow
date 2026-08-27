CREATE OR ALTER PROCEDURE dbo.usp_OfficialHolidayDefinitions_Delete
    @OfficialHolidayDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.OfficialHolidayDefinitions WHERE Id = @OfficialHolidayDefinitionId)
    BEGIN
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    DELETE FROM dbo.OfficialHolidayDays
    WHERE OfficialHolidayDefinitionId = @OfficialHolidayDefinitionId;

    DELETE FROM dbo.OfficialHolidayDefinitions
    WHERE Id = @OfficialHolidayDefinitionId;

    SELECT CAST(1 AS bit);
END;
