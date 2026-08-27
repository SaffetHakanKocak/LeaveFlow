CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_ExistsOverlap
    @ConsultantId uniqueidentifier,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.LeaveRequests
            WHERE ConsultantId = @ConsultantId
                AND Status IN (N'Pending', N'Approved')
                AND StartDate <= @EndDate
                AND EndDate >= @StartDate
        )
        THEN 1 ELSE 0 END AS bit) AS ExistsOverlap;
END;
