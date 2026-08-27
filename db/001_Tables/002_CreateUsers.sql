IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_Users_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_Users PRIMARY KEY,
        Email nvarchar(256) NOT NULL,
        NormalizedEmail nvarchar(256) NOT NULL,
        DisplayName nvarchar(160) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail)
    );
END;
