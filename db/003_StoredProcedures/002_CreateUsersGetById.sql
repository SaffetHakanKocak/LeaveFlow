CREATE OR ALTER PROCEDURE dbo.usp_Users_GetById
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Email,
        DisplayName,
        IsActive
    FROM dbo.Users
    WHERE Id = @UserId;
END;
