IF OBJECT_ID(N'dbo.LoginAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAttempts
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginAttempts PRIMARY KEY,
        UserId uniqueidentifier NULL,
        NormalizedEmail nvarchar(256) NULL,
        IpAddress nvarchar(64) NULL,
        Succeeded bit NOT NULL,
        FailureReason nvarchar(128) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_LoginAttempts_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LoginAttempts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
