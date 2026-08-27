CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_Create
    @ConsultantId uniqueidentifier,
    @Reason nvarchar(512),
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate > @EndDate
        THROW 53001, 'Start date must be on or before end date.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Consultants WHERE Id = @ConsultantId AND IsActive = 1)
        THROW 53002, 'Consultant is not active or does not exist.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.LeaveRequests
        WHERE ConsultantId = @ConsultantId
            AND Status IN (N'Pending', N'Approved')
            AND StartDate <= @EndDate
            AND EndDate >= @StartDate
    )
        THROW 53003, 'Leave request overlaps an existing pending or approved request.', 1;

    DECLARE @InsertedIds TABLE (Id uniqueidentifier NOT NULL);
    DECLARE @LeaveRequestId uniqueidentifier;

    INSERT INTO dbo.LeaveRequests (ConsultantId, StartDate, EndDate, Status, Reason)
    OUTPUT INSERTED.Id INTO @InsertedIds
    VALUES (@ConsultantId, @StartDate, @EndDate, N'Pending', @Reason);

    SELECT @LeaveRequestId = Id FROM @InsertedIds;
    SELECT @LeaveRequestId;
END;
