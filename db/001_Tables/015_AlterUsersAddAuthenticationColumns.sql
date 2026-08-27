IF COL_LENGTH(N'dbo.Users', N'PasswordHash') IS NULL
    ALTER TABLE dbo.Users ADD PasswordHash nvarchar(500) NULL;

IF COL_LENGTH(N'dbo.Users', N'FailedLoginCount') IS NULL
    ALTER TABLE dbo.Users ADD FailedLoginCount int NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT 0;

IF COL_LENGTH(N'dbo.Users', N'LockoutEnd') IS NULL
    ALTER TABLE dbo.Users ADD LockoutEnd datetime2(7) NULL;

IF COL_LENGTH(N'dbo.Users', N'LastLoginAt') IS NULL
    ALTER TABLE dbo.Users ADD LastLoginAt datetime2(7) NULL;
