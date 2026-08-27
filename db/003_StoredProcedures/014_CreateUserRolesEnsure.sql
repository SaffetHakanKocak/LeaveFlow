CREATE OR ALTER PROCEDURE dbo.usp_UserRoles_Ensure
    @UserId uniqueidentifier,
    @RoleName nvarchar(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleId int;

    SELECT @RoleId = Id
    FROM dbo.Roles
    WHERE Name = @RoleName;

    IF @RoleId IS NULL
    BEGIN
        THROW 50001, 'The specified role was not found.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles
        WHERE UserId = @UserId
          AND RoleId = @RoleId
    )
    BEGIN
        INSERT INTO dbo.UserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
    END;
END;
