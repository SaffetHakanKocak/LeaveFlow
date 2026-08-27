CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_Reject
    @LeaveRequestId uniqueidentifier,
    @ReviewerUserId uniqueidentifier,
    @ReviewerManagerId uniqueidentifier = NULL,
    @IsAdministrator bit = 0,
    @ReviewNote nvarchar(512) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ConsultantId uniqueidentifier;

    BEGIN TRANSACTION;

    SELECT @ConsultantId = ConsultantId
    FROM dbo.LeaveRequests WITH (UPDLOCK, ROWLOCK)
    WHERE Id = @LeaveRequestId
        AND Status = N'Pending';

    IF @ConsultantId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    IF @IsAdministrator = 0
        AND NOT EXISTS
        (
            SELECT 1
            FROM dbo.ManagerConsultants
            WHERE ManagerId = @ReviewerManagerId
                AND ConsultantId = @ConsultantId
        )
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    UPDATE dbo.LeaveRequests
    SET
        Status = N'Rejected',
        ReviewedAt = SYSUTCDATETIME(),
        ReviewedBy = @ReviewerUserId,
        ReviewNote = NULLIF(LTRIM(RTRIM(@ReviewNote)), N''),
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @LeaveRequestId
        AND Status = N'Pending';

    IF @@ROWCOUNT <> 1
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT CAST(0 AS bit);
        RETURN;
    END;

    COMMIT TRANSACTION;
    SELECT CAST(1 AS bit);
END;
