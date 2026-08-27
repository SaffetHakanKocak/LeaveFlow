CREATE OR ALTER PROCEDURE dbo.usp_Users_UpdatePasswordHash
    @UserId uniqueidentifier,
    @PasswordHash nvarchar(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET
        PasswordHash = @PasswordHash,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @UserId;
END;
