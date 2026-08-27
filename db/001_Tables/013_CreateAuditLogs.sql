IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        ActorUserId uniqueidentifier NULL,
        Action nvarchar(128) NOT NULL,
        TargetType nvarchar(128) NULL,
        TargetId nvarchar(128) NULL,
        Outcome nvarchar(32) NOT NULL,
        CorrelationId nvarchar(128) NULL,
        MetadataJson nvarchar(2000) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_AuditLogs_Outcome CHECK (Outcome IN (N'Success', N'Failure', N'Denied'))
    );
END;
