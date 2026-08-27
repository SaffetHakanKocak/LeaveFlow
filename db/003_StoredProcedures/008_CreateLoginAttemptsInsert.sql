CREATE OR ALTER PROCEDURE dbo.usp_LoginAttempts_Insert
    @UserId uniqueidentifier = NULL,
    @NormalizedEmail nvarchar(256) = NULL,
    @IpAddress nvarchar(64) = NULL,
    @Succeeded bit,
    @FailureReason nvarchar(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.LoginAttempts
    (
        UserId,
        NormalizedEmail,
        IpAddress,
        Succeeded,
        FailureReason
    )
    VALUES
    (
        @UserId,
        @NormalizedEmail,
        @IpAddress,
        @Succeeded,
        @FailureReason
    );
END;
