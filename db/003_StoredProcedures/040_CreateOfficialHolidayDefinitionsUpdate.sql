CREATE OR ALTER PROCEDURE dbo.usp_OfficialHolidayDefinitions_Update
    @OfficialHolidayDefinitionId uniqueidentifier,
    @Name nvarchar(160),
    @StartDate date,
    @EndDate date,
    @CountryCode char(2) = 'ZZ',
    @RegionCode nvarchar(32) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate > @EndDate
        THROW 52007, 'Start date must be on or before end date.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.OfficialHolidayDefinitions WHERE Id = @OfficialHolidayDefinitionId)
    BEGIN
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.OfficialHolidayDefinitions
        WHERE Id <> @OfficialHolidayDefinitionId
            AND CountryCode = @CountryCode
            AND ISNULL(RegionCode, N'') = ISNULL(@RegionCode, N'')
            AND Name = @Name
            AND StartDate = @StartDate
            AND EndDate = @EndDate
    )
        THROW 52008, 'Official holiday already exists for this scope, name, and date range.', 1;

    DECLARE @CurrentDate date = @StartDate;

    UPDATE dbo.OfficialHolidayDefinitions
    SET
        CountryCode = @CountryCode,
        RegionCode = @RegionCode,
        Name = @Name,
        StartDate = @StartDate,
        EndDate = @EndDate,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @OfficialHolidayDefinitionId;

    DELETE FROM dbo.OfficialHolidayDays
    WHERE OfficialHolidayDefinitionId = @OfficialHolidayDefinitionId;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO dbo.OfficialHolidayDays (OfficialHolidayDefinitionId, HolidayDate, Name)
        VALUES (@OfficialHolidayDefinitionId, @CurrentDate, @Name);

        SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
    END;

    SELECT CAST(1 AS bit);
END;
