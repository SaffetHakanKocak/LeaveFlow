CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_SetActive
    @HolidayDefinitionId uniqueidentifier,
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.HolidayDefinitions
    SET IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @HolidayDefinitionId;

    SELECT CAST(CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END AS bit);
END;
