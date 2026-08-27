IF OBJECT_ID(N'dbo.OfficialHolidayDays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficialHolidayDays
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_OfficialHolidayDays PRIMARY KEY,
        OfficialHolidayDefinitionId uniqueidentifier NOT NULL,
        HolidayDate date NOT NULL,
        Name nvarchar(160) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_OfficialHolidayDays_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_OfficialHolidayDays_OfficialHolidayDefinitions FOREIGN KEY (OfficialHolidayDefinitionId) REFERENCES dbo.OfficialHolidayDefinitions(Id),
        CONSTRAINT UQ_OfficialHolidayDays_DefinitionDate UNIQUE (OfficialHolidayDefinitionId, HolidayDate)
    );
END;
