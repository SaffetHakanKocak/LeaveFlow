IF OBJECT_ID(N'dbo.ConsultantLeaveDays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsultantLeaveDays
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsultantLeaveDays PRIMARY KEY,
        LeaveRequestId uniqueidentifier NOT NULL,
        ConsultantId uniqueidentifier NOT NULL,
        LeaveDate date NOT NULL,
        Duration decimal(4,2) NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_ConsultantLeaveDays_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ConsultantLeaveDays_LeaveRequests FOREIGN KEY (LeaveRequestId) REFERENCES dbo.LeaveRequests(Id),
        CONSTRAINT FK_ConsultantLeaveDays_Consultants FOREIGN KEY (ConsultantId) REFERENCES dbo.Consultants(Id),
        CONSTRAINT UQ_ConsultantLeaveDays_RequestDate UNIQUE (LeaveRequestId, LeaveDate),
        CONSTRAINT CK_ConsultantLeaveDays_Duration CHECK (Duration > 0 AND Duration <= 1)
    );
END;
