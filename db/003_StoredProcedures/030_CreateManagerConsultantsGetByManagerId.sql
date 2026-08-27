CREATE OR ALTER PROCEDURE dbo.usp_ManagerConsultants_GetByManagerId
    @ManagerId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        mc.ManagerId,
        mc.ConsultantId,
        CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
        u.Email AS ConsultantEmail,
        c.IsActive
    FROM dbo.ManagerConsultants AS mc
    INNER JOIN dbo.Consultants AS c ON c.Id = mc.ConsultantId
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE mc.ManagerId = @ManagerId
    ORDER BY c.LastName, c.FirstName;
END;
