IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsultantLeaveDays_Date_Consultant' AND object_id = OBJECT_ID(N'dbo.ConsultantLeaveDays'))
    CREATE INDEX IX_ConsultantLeaveDays_Date_Consultant ON dbo.ConsultantLeaveDays(LeaveDate, ConsultantId) INCLUDE (LeaveRequestId);
