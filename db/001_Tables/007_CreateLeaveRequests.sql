IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveRequests
    (
        Id uniqueidentifier NOT NULL CONSTRAINT DF_LeaveRequests_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_LeaveRequests PRIMARY KEY,
        ConsultantId uniqueidentifier NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        Reason nvarchar(512) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_LeaveRequests_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(7) NULL,
        CONSTRAINT FK_LeaveRequests_Consultants FOREIGN KEY (ConsultantId) REFERENCES dbo.Consultants(Id),
        CONSTRAINT CK_LeaveRequests_DateRange CHECK (StartDate <= EndDate),
        CONSTRAINT CK_LeaveRequests_Status CHECK (Status IN (N'Draft', N'Pending', N'Approved', N'Rejected', N'Cancelled'))
    );
END;
