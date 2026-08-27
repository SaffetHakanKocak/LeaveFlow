CREATE OR ALTER PROCEDURE dbo.usp_Users_RecordFailedLogin
    @UserId uniqueidentifier,
    @MaxFailedAccessAttempts int,
    @LockoutDurationMinutes int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now datetime2(7) = SYSUTCDATETIME();
    DECLARE @FailedLoginCount int;
    DECLARE @LockoutEnd datetime2(7);
    DECLARE @LockoutApplied bit = 0;

    SELECT
        @FailedLoginCount = FailedLoginCount,
        @LockoutEnd = LockoutEnd
    FROM dbo.Users
    WHERE Id = @UserId;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
    BEGIN
        RETURN;
    END;

    IF @LockoutEnd IS NOT NULL AND @LockoutEnd > @Now
    BEGIN
        SELECT
            @FailedLoginCount AS FailedLoginCount,
            @LockoutEnd AS LockoutEnd,
            CAST(0 AS bit) AS LockoutApplied;
        RETURN;
    END;

    IF @LockoutEnd IS NOT NULL AND @LockoutEnd <= @Now
    BEGIN
        SET @FailedLoginCount = 0;
        SET @LockoutEnd = NULL;
    END;

    SET @FailedLoginCount = @FailedLoginCount + 1;

    IF @FailedLoginCount >= @MaxFailedAccessAttempts
    BEGIN
        SET @LockoutEnd = DATEADD(minute, @LockoutDurationMinutes, @Now);
        SET @LockoutApplied = 1;
    END;

    UPDATE dbo.Users
    SET
        FailedLoginCount = @FailedLoginCount,
        LockoutEnd = @LockoutEnd,
        UpdatedAt = @Now
    WHERE Id = @UserId;

    SELECT
        @FailedLoginCount AS FailedLoginCount,
        @LockoutEnd AS LockoutEnd,
        @LockoutApplied AS LockoutApplied;
END;
