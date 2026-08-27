IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_Consultant_Status_Range' AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
    CREATE INDEX IX_LeaveRequests_Consultant_Status_Range ON dbo.LeaveRequests(ConsultantId, Status, StartDate, EndDate);
