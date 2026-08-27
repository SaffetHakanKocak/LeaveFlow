CREATE OR ALTER PROCEDURE dbo.usp_Managers_EnsureForUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ManagerId uniqueidentifier;

    SELECT @ManagerId = Id
    FROM dbo.Managers
    WHERE UserId = @UserId;

    IF @ManagerId IS NULL
    BEGIN
        INSERT INTO dbo.Managers (UserId)
        VALUES (@UserId);

        SELECT @ManagerId = Id
        FROM dbo.Managers
        WHERE UserId = @UserId;
    END;

    SELECT @ManagerId AS Id;
END;
