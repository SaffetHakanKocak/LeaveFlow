CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_GetForReview
    @LeaveRequestId uniqueidentifier,
    @ReviewerUserId uniqueidentifier,
    @ReviewerManagerId uniqueidentifier = NULL,
    @IsAdministrator bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lr.Id,
        lr.ConsultantId,
        CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
        u.Email AS ConsultantEmail,
        lr.Reason,
        lr.StartDate,
        lr.EndDate,
        lr.Status,
        lr.CreatedAt,
        lr.ReviewedAt,
        lr.ReviewedBy,
        lr.ReviewNote
    FROM dbo.LeaveRequests AS lr
    INNER JOIN dbo.Consultants AS c ON c.Id = lr.ConsultantId
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE lr.Id = @LeaveRequestId
        AND
        (
            @IsAdministrator = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.ManagerConsultants AS mc
                WHERE mc.ManagerId = @ReviewerManagerId
                    AND mc.ConsultantId = lr.ConsultantId
            )
        );
END;
