IF OBJECT_ID(N'dbo.Consultants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consultants
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_Consultants_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_Consultants PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        EmployeeNumber nvarchar(64) NULL,
        StartDate date NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Consultants_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        CONSTRAINT UQ_Consultants_UserId UNIQUE (UserId),
        CONSTRAINT UQ_Consultants_EmployeeNumber UNIQUE (EmployeeNumber),
        CONSTRAINT FK_Consultants_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
