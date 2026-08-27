IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HolidayDefinitions_Active_Dates_Name' AND object_id = OBJECT_ID(N'dbo.HolidayDefinitions'))
    CREATE INDEX IX_HolidayDefinitions_Active_Dates_Name ON dbo.HolidayDefinitions(IsActive, StartDate, EndDate, Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OfficialHolidayDefinitions_Dates_Name' AND object_id = OBJECT_ID(N'dbo.OfficialHolidayDefinitions'))
    CREATE INDEX IX_OfficialHolidayDefinitions_Dates_Name ON dbo.OfficialHolidayDefinitions(StartDate, EndDate, Name);
