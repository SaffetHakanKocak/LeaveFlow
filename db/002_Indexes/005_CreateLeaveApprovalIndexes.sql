IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ConsultantLeaveDays_Consultant_Request_Date' AND object_id = OBJECT_ID(N'dbo.ConsultantLeaveDays'))
    CREATE UNIQUE INDEX UX_ConsultantLeaveDays_Consultant_Request_Date ON dbo.ConsultantLeaveDays(ConsultantId, LeaveRequestId, LeaveDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsultantLeaveDays_Consultant_Date' AND object_id = OBJECT_ID(N'dbo.ConsultantLeaveDays'))
    CREATE INDEX IX_ConsultantLeaveDays_Consultant_Date ON dbo.ConsultantLeaveDays(ConsultantId, LeaveDate);
