IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        Name nvarchar(64) NOT NULL,
        Description nvarchar(256) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END;
