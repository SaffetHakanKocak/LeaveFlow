IF OBJECT_ID(N'dbo.Managers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Managers
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_Managers_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_Managers PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Managers_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        CONSTRAINT UQ_Managers_UserId UNIQUE (UserId),
        CONSTRAINT FK_Managers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
