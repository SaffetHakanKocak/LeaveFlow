CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_GetPendingForAdmin
    @ConsultantId uniqueidentifier = NULL,
    @FromDate date = NULL,
    @ToDate date = NULL,
    @Status nvarchar(32) = N'Pending',
    @PageNumber int = 1,
    @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset int = (@PageNumber - 1) * @PageSize;

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
        COUNT(1) OVER() AS TotalCount
    FROM dbo.LeaveRequests AS lr
    INNER JOIN dbo.Consultants AS c ON c.Id = lr.ConsultantId
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE (@ConsultantId IS NULL OR lr.ConsultantId = @ConsultantId)
        AND (@Status IS NULL OR lr.Status = @Status)
        AND (@FromDate IS NULL OR lr.EndDate >= @FromDate)
        AND (@ToDate IS NULL OR lr.StartDate <= @ToDate)
    ORDER BY lr.CreatedAt ASC, lr.StartDate ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
