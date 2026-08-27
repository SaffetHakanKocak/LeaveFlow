CREATE OR ALTER PROCEDURE dbo.usp_Roles_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Description,
        IsActive
    FROM dbo.Roles
    WHERE IsActive = 1
    ORDER BY Name;
END;
