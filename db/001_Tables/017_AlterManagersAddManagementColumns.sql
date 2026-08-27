IF COL_LENGTH(N'dbo.Managers', N'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Managers ADD FirstName nvarchar(80) NULL;
END;

IF COL_LENGTH(N'dbo.Managers', N'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Managers ADD LastName nvarchar(80) NULL;
END;

IF COL_LENGTH(N'dbo.Managers', N'Department') IS NULL
BEGIN
    ALTER TABLE dbo.Managers ADD Department nvarchar(120) NULL;
END;

IF COL_LENGTH(N'dbo.Managers', N'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Managers ADD IsActive bit NOT NULL CONSTRAINT DF_Managers_IsActive DEFAULT 1;
END;

UPDATE m
SET
    FirstName = COALESCE(m.FirstName, LEFT(u.DisplayName, 80)),
    LastName = COALESCE(m.LastName, N'Profile')
FROM dbo.Managers AS m
INNER JOIN dbo.Users AS u ON u.Id = m.UserId
WHERE m.FirstName IS NULL OR m.LastName IS NULL;

IF COL_LENGTH(N'dbo.Managers', N'FirstName') IS NOT NULL
    ALTER TABLE dbo.Managers ALTER COLUMN FirstName nvarchar(80) NOT NULL;

IF COL_LENGTH(N'dbo.Managers', N'LastName') IS NOT NULL
    ALTER TABLE dbo.Managers ALTER COLUMN LastName nvarchar(80) NOT NULL;
