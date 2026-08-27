IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserRoles_RoleId' AND object_id = OBJECT_ID(N'dbo.UserRoles'))
    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles(RoleId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ManagerConsultants_ConsultantId' AND object_id = OBJECT_ID(N'dbo.ManagerConsultants'))
    CREATE INDEX IX_ManagerConsultants_ConsultantId ON dbo.ManagerConsultants(ConsultantId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_Consultant_Status_Dates' AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
    CREATE INDEX IX_LeaveRequests_Consultant_Status_Dates ON dbo.LeaveRequests(ConsultantId, Status, StartDate, EndDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsultantLeaveDays_Consultant_Date' AND object_id = OBJECT_ID(N'dbo.ConsultantLeaveDays'))
    CREATE INDEX IX_ConsultantLeaveDays_Consultant_Date ON dbo.ConsultantLeaveDays(ConsultantId, LeaveDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HolidayDays_Date' AND object_id = OBJECT_ID(N'dbo.HolidayDays'))
    CREATE INDEX IX_HolidayDays_Date ON dbo.HolidayDays(HolidayDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OfficialHolidayDays_Date' AND object_id = OBJECT_ID(N'dbo.OfficialHolidayDays'))
    CREATE INDEX IX_OfficialHolidayDays_Date ON dbo.OfficialHolidayDays(HolidayDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Actor_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_Actor_CreatedAt ON dbo.AuditLogs(ActorUserId, CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Target_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_Target_CreatedAt ON dbo.AuditLogs(TargetType, TargetId, CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LoginAttempts_Email_CreatedAt' AND object_id = OBJECT_ID(N'dbo.LoginAttempts'))
    CREATE INDEX IX_LoginAttempts_Email_CreatedAt ON dbo.LoginAttempts(NormalizedEmail, CreatedAt);
