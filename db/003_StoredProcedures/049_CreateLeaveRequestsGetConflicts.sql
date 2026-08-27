CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_GetConflicts
    @LeaveRequestId uniqueidentifier,
    @ReviewerUserId uniqueidentifier,
    @ReviewerManagerId uniqueidentifier = NULL,
    @IsAdministrator bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate date;
    DECLARE @EndDate date;
    DECLARE @ConsultantId uniqueidentifier;

    SELECT
        @StartDate = StartDate,
        @EndDate = EndDate,
        @ConsultantId = ConsultantId
    FROM dbo.LeaveRequests
    WHERE Id = @LeaveRequestId;

    IF @StartDate IS NULL
        RETURN;

    IF @IsAdministrator = 0
        AND NOT EXISTS
        (
            SELECT 1
            FROM dbo.ManagerConsultants
            WHERE ManagerId = @ReviewerManagerId
                AND ConsultantId = @ConsultantId
        )
        RETURN;

    SELECT
        c.Id AS ConsultantId,
        CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
        u.Email AS ConsultantEmail,
        cld.LeaveDate AS ConflictDate
    FROM dbo.ConsultantLeaveDays AS cld
    INNER JOIN dbo.LeaveRequests AS existing ON existing.Id = cld.LeaveRequestId
    INNER JOIN dbo.Consultants AS c ON c.Id = cld.ConsultantId
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE existing.Id <> @LeaveRequestId
        AND existing.Status = N'Approved'
        AND cld.LeaveDate BETWEEN @StartDate AND @EndDate
    ORDER BY c.LastName, c.FirstName, cld.LeaveDate;
END;
