CREATE OR ALTER PROCEDURE dbo.usp_OfficialHolidayDefinitions_Create
    @Name nvarchar(160),
    @StartDate date,
    @EndDate date,
    @CountryCode char(2) = 'ZZ',
    @RegionCode nvarchar(32) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate > @EndDate
        THROW 52005, 'Start date must be on or before end date.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.OfficialHolidayDefinitions
        WHERE CountryCode = @CountryCode
            AND ISNULL(RegionCode, N'') = ISNULL(@RegionCode, N'')
            AND Name = @Name
            AND StartDate = @StartDate
            AND EndDate = @EndDate
    )
        THROW 52006, 'Official holiday already exists for this scope, name, and date range.', 1;

    DECLARE @InsertedIds TABLE (Id uniqueidentifier NOT NULL);
    DECLARE @OfficialHolidayDefinitionId uniqueidentifier;
    DECLARE @CurrentDate date = @StartDate;

    INSERT INTO dbo.OfficialHolidayDefinitions (CountryCode, RegionCode, Name, StartDate, EndDate, IsActive)
    OUTPUT INSERTED.Id INTO @InsertedIds
    VALUES (@CountryCode, @RegionCode, @Name, @StartDate, @EndDate, 1);

    SELECT @OfficialHolidayDefinitionId = Id FROM @InsertedIds;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO dbo.OfficialHolidayDays (OfficialHolidayDefinitionId, HolidayDate, Name)
        VALUES (@OfficialHolidayDefinitionId, @CurrentDate, @Name);

        SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
    END;

    SELECT @OfficialHolidayDefinitionId;
END;
