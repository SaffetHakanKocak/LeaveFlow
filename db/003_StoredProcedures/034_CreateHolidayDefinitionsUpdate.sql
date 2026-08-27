CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_Update
    @HolidayDefinitionId uniqueidentifier,
    @Name nvarchar(160),
    @StartDate date,
    @EndDate date,
    @IsActive bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate > @EndDate
        THROW 52003, 'Start date must be on or before end date.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.HolidayDefinitions WHERE Id = @HolidayDefinitionId)
    BEGIN
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.HolidayDefinitions
        WHERE Id <> @HolidayDefinitionId
            AND Name = @Name
            AND StartDate = @StartDate
            AND EndDate = @EndDate
    )
        THROW 52004, 'Holiday already exists for this name and date range.', 1;

    DECLARE @CurrentDate date = @StartDate;

    UPDATE dbo.HolidayDefinitions
    SET
        Name = @Name,
        StartDate = @StartDate,
        EndDate = @EndDate,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @HolidayDefinitionId;

    DELETE FROM dbo.HolidayDays
    WHERE HolidayDefinitionId = @HolidayDefinitionId;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO dbo.HolidayDays (HolidayDefinitionId, HolidayDate, Name)
        VALUES (@HolidayDefinitionId, @CurrentDate, @Name);

        SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
    END;

    SELECT CAST(1 AS bit);
END;
