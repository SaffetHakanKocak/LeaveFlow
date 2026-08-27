CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_GetById
    @LeaveRequestId uniqueidentifier,
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        ConsultantId,
        Reason,
        StartDate,
        EndDate,
        Status,
        CreatedAt,
        ReviewedAt,
        ReviewedBy,
        ReviewNote
    FROM dbo.LeaveRequests
    WHERE Id = @LeaveRequestId
        AND ConsultantId = @ConsultantId;
END;
