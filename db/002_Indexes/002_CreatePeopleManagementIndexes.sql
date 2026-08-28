IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Consultants_IsActive_LastName' AND object_id = OBJECT_ID(N'dbo.Consultants'))
    CREATE INDEX IX_Consultants_IsActive_LastName ON dbo.Consultants(IsActive, LastName, FirstName);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Consultants_EmployeeNumber' AND object_id = OBJECT_ID(N'dbo.Consultants'))
    CREATE UNIQUE INDEX UQ_Consultants_EmployeeNumber ON dbo.Consultants(EmployeeNumber) WHERE EmployeeNumber IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Managers_IsActive_LastName' AND object_id = OBJECT_ID(N'dbo.Managers'))
    CREATE INDEX IX_Managers_IsActive_LastName ON dbo.Managers(IsActive, LastName, FirstName);
