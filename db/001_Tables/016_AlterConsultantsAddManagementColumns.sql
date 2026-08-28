IF COL_LENGTH(N'dbo.Consultants', N'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Consultants ADD FirstName nvarchar(80) NULL;
END;

IF COL_LENGTH(N'dbo.Consultants', N'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Consultants ADD LastName nvarchar(80) NULL;
END;

IF COL_LENGTH(N'dbo.Consultants', N'Department') IS NULL
BEGIN
    ALTER TABLE dbo.Consultants ADD Department nvarchar(120) NULL;
END;

IF COL_LENGTH(N'dbo.Consultants', N'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Consultants ADD IsActive bit NOT NULL CONSTRAINT DF_Consultants_IsActive DEFAULT 1;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_Consultants_EmployeeNumber'
        AND parent_object_id = OBJECT_ID(N'dbo.Consultants')
)
BEGIN
    ALTER TABLE dbo.Consultants DROP CONSTRAINT UQ_Consultants_EmployeeNumber;
END;

GO

UPDATE c
SET
    FirstName = COALESCE(c.FirstName, LEFT(u.DisplayName, 80)),
    LastName = COALESCE(c.LastName, N'Profile')
FROM dbo.Consultants AS c
INNER JOIN dbo.Users AS u ON u.Id = c.UserId
WHERE c.FirstName IS NULL OR c.LastName IS NULL;

IF COL_LENGTH(N'dbo.Consultants', N'FirstName') IS NOT NULL
    ALTER TABLE dbo.Consultants ALTER COLUMN FirstName nvarchar(80) NOT NULL;

IF COL_LENGTH(N'dbo.Consultants', N'LastName') IS NOT NULL
    ALTER TABLE dbo.Consultants ALTER COLUMN LastName nvarchar(80) NOT NULL;
