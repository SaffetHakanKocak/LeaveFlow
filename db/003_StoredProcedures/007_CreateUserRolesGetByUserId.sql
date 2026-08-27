CREATE OR ALTER PROCEDURE dbo.usp_UserRoles_GetByUserId
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.Name
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Roles r ON r.Id = ur.RoleId
    WHERE ur.UserId = @UserId
      AND r.IsActive = 1
    ORDER BY r.Name;
END;
