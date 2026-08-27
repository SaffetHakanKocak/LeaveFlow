IF OBJECT_ID(N'dbo.HolidayDays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HolidayDays
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_HolidayDays PRIMARY KEY,
        HolidayDefinitionId uniqueidentifier NOT NULL,
        HolidayDate date NOT NULL,
        Name nvarchar(160) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_HolidayDays_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_HolidayDays_HolidayDefinitions FOREIGN KEY (HolidayDefinitionId) REFERENCES dbo.HolidayDefinitions(Id),
        CONSTRAINT UQ_HolidayDays_DefinitionDate UNIQUE (HolidayDefinitionId, HolidayDate)
    );
END;
