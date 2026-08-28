IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_LeaveRequests_Status_Date_Consultant'
        AND object_id = OBJECT_ID(N'dbo.LeaveRequests')
)
BEGIN
    CREATE INDEX IX_LeaveRequests_Status_Date_Consultant
        ON dbo.LeaveRequests(Status, StartDate, EndDate, ConsultantId);
END;
