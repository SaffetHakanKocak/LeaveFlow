CREATE OR ALTER PROCEDURE dbo.usp_LeaveRequests_Approve
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
    DECLARE @StartDate date;
    DECLARE @EndDate date;
    DECLARE @CurrentDate date;

    BEGIN TRANSACTION;

    SELECT
        @ConsultantId = ConsultantId,
        @StartDate = StartDate,
        @EndDate = EndDate
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
        Status = N'Approved',
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

    SET @CurrentDate = @StartDate;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO dbo.ConsultantLeaveDays (LeaveRequestId, ConsultantId, LeaveDate, Duration)
        VALUES (@LeaveRequestId, @ConsultantId, @CurrentDate, 1.00);

        SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
    END;

    COMMIT TRANSACTION;
    SELECT CAST(1 AS bit);
END;
