CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_Create
    @Name nvarchar(160),
    @StartDate date,
    @EndDate date,
    @IsActive bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate > @EndDate
        THROW 52001, 'Start date must be on or before end date.', 1;

    IF EXISTS (SELECT 1 FROM dbo.HolidayDefinitions WHERE Name = @Name AND StartDate = @StartDate AND EndDate = @EndDate)
        THROW 52002, 'Holiday already exists for this name and date range.', 1;

    DECLARE @InsertedIds TABLE (Id uniqueidentifier NOT NULL);
    DECLARE @HolidayDefinitionId uniqueidentifier;
    DECLARE @CurrentDate date = @StartDate;

    INSERT INTO dbo.HolidayDefinitions (Name, StartDate, EndDate, IsActive)
    OUTPUT INSERTED.Id INTO @InsertedIds
    VALUES (@Name, @StartDate, @EndDate, @IsActive);

    SELECT @HolidayDefinitionId = Id FROM @InsertedIds;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO dbo.HolidayDays (HolidayDefinitionId, HolidayDate, Name)
        VALUES (@HolidayDefinitionId, @CurrentDate, @Name);

        SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
    END;

    SELECT @HolidayDefinitionId;
END;
