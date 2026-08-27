IF OBJECT_ID(N'dbo.OfficialHolidayDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficialHolidayDefinitions
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_OfficialHolidayDefinitions_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_OfficialHolidayDefinitions PRIMARY KEY,
        CountryCode char(2) NOT NULL,
        RegionCode nvarchar(32) NULL,
        Name nvarchar(160) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_OfficialHolidayDefinitions_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_OfficialHolidayDefinitions_IsActive DEFAULT 1,
        CONSTRAINT UQ_OfficialHolidayDefinitions_ScopeName UNIQUE (CountryCode, RegionCode, Name)
    );
END;
