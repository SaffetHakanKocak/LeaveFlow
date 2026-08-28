IF COL_LENGTH(N'dbo.HolidayDefinitions', N'StartDate') IS NULL
BEGIN
    ALTER TABLE dbo.HolidayDefinitions ADD StartDate date NULL;
END;

IF COL_LENGTH(N'dbo.HolidayDefinitions', N'EndDate') IS NULL
BEGIN
    ALTER TABLE dbo.HolidayDefinitions ADD EndDate date NULL;
END;

GO

UPDATE hd
SET
    StartDate = COALESCE(hd.StartDate, days.StartDate, CONVERT(date, hd.CreatedAt)),
    EndDate = COALESCE(hd.EndDate, days.EndDate, CONVERT(date, hd.CreatedAt))
FROM dbo.HolidayDefinitions AS hd
OUTER APPLY
(
    SELECT MIN(HolidayDate) AS StartDate, MAX(HolidayDate) AS EndDate
    FROM dbo.HolidayDays
    WHERE HolidayDefinitionId = hd.Id
) AS days
WHERE hd.StartDate IS NULL OR hd.EndDate IS NULL;

ALTER TABLE dbo.HolidayDefinitions ALTER COLUMN StartDate date NOT NULL;
ALTER TABLE dbo.HolidayDefinitions ALTER COLUMN EndDate date NOT NULL;

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_HolidayDefinitions_Name'
        AND parent_object_id = OBJECT_ID(N'dbo.HolidayDefinitions')
)
BEGIN
    ALTER TABLE dbo.HolidayDefinitions DROP CONSTRAINT UQ_HolidayDefinitions_Name;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UQ_HolidayDefinitions_Name_DateRange'
        AND object_id = OBJECT_ID(N'dbo.HolidayDefinitions')
)
BEGIN
    CREATE UNIQUE INDEX UQ_HolidayDefinitions_Name_DateRange
        ON dbo.HolidayDefinitions(Name, StartDate, EndDate);
END;
