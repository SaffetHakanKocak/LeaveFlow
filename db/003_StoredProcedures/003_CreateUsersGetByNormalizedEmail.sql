CREATE OR ALTER PROCEDURE dbo.usp_Users_GetByNormalizedEmail
    @NormalizedEmail nvarchar(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Email,
        DisplayName,
        PasswordHash,
        IsActive,
        FailedLoginCount,
        LockoutEnd,
        LastLoginAt
    FROM dbo.Users
    WHERE NormalizedEmail = @NormalizedEmail;
END;
