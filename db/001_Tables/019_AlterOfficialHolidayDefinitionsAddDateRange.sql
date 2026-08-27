IF COL_LENGTH(N'dbo.OfficialHolidayDefinitions', N'StartDate') IS NULL
BEGIN
    ALTER TABLE dbo.OfficialHolidayDefinitions ADD StartDate date NULL;
END;

IF COL_LENGTH(N'dbo.OfficialHolidayDefinitions', N'EndDate') IS NULL
BEGIN
    ALTER TABLE dbo.OfficialHolidayDefinitions ADD EndDate date NULL;
END;

UPDATE hd
SET
    StartDate = COALESCE(hd.StartDate, days.StartDate, CONVERT(date, hd.CreatedAt)),
    EndDate = COALESCE(hd.EndDate, days.EndDate, CONVERT(date, hd.CreatedAt))
FROM dbo.OfficialHolidayDefinitions AS hd
OUTER APPLY
(
    SELECT MIN(HolidayDate) AS StartDate, MAX(HolidayDate) AS EndDate
    FROM dbo.OfficialHolidayDays
    WHERE OfficialHolidayDefinitionId = hd.Id
) AS days
WHERE hd.StartDate IS NULL OR hd.EndDate IS NULL;

ALTER TABLE dbo.OfficialHolidayDefinitions ALTER COLUMN StartDate date NOT NULL;
ALTER TABLE dbo.OfficialHolidayDefinitions ALTER COLUMN EndDate date NOT NULL;

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_OfficialHolidayDefinitions_ScopeName'
        AND parent_object_id = OBJECT_ID(N'dbo.OfficialHolidayDefinitions')
)
BEGIN
    ALTER TABLE dbo.OfficialHolidayDefinitions DROP CONSTRAINT UQ_OfficialHolidayDefinitions_ScopeName;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_OfficialHolidayDefinitions_ScopeName_DateRange'
        AND object_id = OBJECT_ID(N'dbo.OfficialHolidayDefinitions')
)
BEGIN
    CREATE UNIQUE INDEX UQ_OfficialHolidayDefinitions_ScopeName_DateRange
        ON dbo.OfficialHolidayDefinitions(CountryCode, RegionCode, Name, StartDate, EndDate);
END;
