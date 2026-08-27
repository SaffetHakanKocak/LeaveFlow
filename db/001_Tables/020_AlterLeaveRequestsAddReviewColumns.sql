IF COL_LENGTH(N'dbo.LeaveRequests', N'ReviewedAt') IS NULL
BEGIN
    ALTER TABLE dbo.LeaveRequests ADD ReviewedAt datetime2(7) NULL;
END;

IF COL_LENGTH(N'dbo.LeaveRequests', N'ReviewedBy') IS NULL
BEGIN
    ALTER TABLE dbo.LeaveRequests ADD ReviewedBy uniqueidentifier NULL;
END;

IF COL_LENGTH(N'dbo.LeaveRequests', N'ReviewNote') IS NULL
BEGIN
    ALTER TABLE dbo.LeaveRequests ADD ReviewNote nvarchar(512) NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_LeaveRequests_ReviewedBy_Users'
        AND parent_object_id = OBJECT_ID(N'dbo.LeaveRequests')
)
BEGIN
    ALTER TABLE dbo.LeaveRequests
        ADD CONSTRAINT FK_LeaveRequests_ReviewedBy_Users FOREIGN KEY (ReviewedBy) REFERENCES dbo.Users(Id);
END;
