CREATE OR ALTER PROCEDURE dbo.usp_Users_UpdateLoginSuccess
    @UserId uniqueidentifier,
    @LastLoginAt datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET
        FailedLoginCount = 0,
        LockoutEnd = NULL,
        LastLoginAt = @LastLoginAt,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @UserId;
END;
