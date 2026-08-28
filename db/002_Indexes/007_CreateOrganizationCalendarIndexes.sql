IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_HolidayDays_Date_Definition'
        AND object_id = OBJECT_ID(N'dbo.HolidayDays')
)
BEGIN
    CREATE INDEX IX_HolidayDays_Date_Definition
        ON dbo.HolidayDays(HolidayDate, HolidayDefinitionId)
        INCLUDE (Name);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_OfficialHolidayDays_Date_Definition'
        AND object_id = OBJECT_ID(N'dbo.OfficialHolidayDays')
)
BEGIN
    CREATE INDEX IX_OfficialHolidayDays_Date_Definition
        ON dbo.OfficialHolidayDays(HolidayDate, OfficialHolidayDefinitionId)
        INCLUDE (Name);
END;
