IF OBJECT_ID(N'dbo.HolidayDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HolidayDefinitions
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_HolidayDefinitions_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_HolidayDefinitions PRIMARY KEY,
        Name nvarchar(160) NOT NULL,
        Description nvarchar(512) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_HolidayDefinitions_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_HolidayDefinitions_IsActive DEFAULT 1,
        CONSTRAINT UQ_HolidayDefinitions_Name UNIQUE (Name)
    );
END;
