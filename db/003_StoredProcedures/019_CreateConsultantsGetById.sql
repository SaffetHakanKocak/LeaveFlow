CREATE OR ALTER PROCEDURE dbo.usp_Consultants_GetById
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id,
        c.UserId,
        c.FirstName,
        c.LastName,
        u.Email,
        c.EmployeeNumber,
        c.Department,
        c.StartDate,
        c.IsActive
    FROM dbo.Consultants AS c
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE c.Id = @ConsultantId;
END;
