CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_GetMine
    @ConsultantId uniqueidentifier,
    @Status nvarchar(32) = NULL,
    @FromDate date = NULL,
    @ToDate date = NULL,
    @PageNumber int = 1,
    @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset int = (@PageNumber - 1) * @PageSize;

    SELECT
        Id,
        ConsultantId,
        Reason,
        StartDate,
        EndDate,
        Status,
        CreatedAt,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.LeaveRequests
    WHERE ConsultantId = @ConsultantId
        AND (@Status IS NULL OR Status = @Status)
        AND (@FromDate IS NULL OR EndDate >= @FromDate)
        AND (@ToDate IS NULL OR StartDate <= @ToDate)
    ORDER BY CreatedAt DESC, StartDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
