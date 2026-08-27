CREATE OR ALTER PROCEDURE dbo.usp_Managers_GetById
    @ManagerId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.Id,
        m.UserId,
        m.FirstName,
        m.LastName,
        u.Email,
        m.Department,
        m.IsActive
    FROM dbo.Managers AS m
    INNER JOIN dbo.Users AS u ON u.Id = m.UserId
    WHERE m.Id = @ManagerId;
END;
