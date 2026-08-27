IF OBJECT_ID(N'dbo.ManagerConsultants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ManagerConsultants
    (
        ManagerId uniqueidentifier NOT NULL,
        ConsultantId uniqueidentifier NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_ManagerConsultants_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ManagerConsultants PRIMARY KEY (ManagerId, ConsultantId),
        CONSTRAINT FK_ManagerConsultants_Managers FOREIGN KEY (ManagerId) REFERENCES dbo.Managers(Id),
        CONSTRAINT FK_ManagerConsultants_Consultants FOREIGN KEY (ConsultantId) REFERENCES dbo.Consultants(Id)
    );
END;
