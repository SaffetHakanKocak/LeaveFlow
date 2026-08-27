CREATE OR ALTER PROCEDURE dbo.usp_Managers_GetIdByUserId
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id
    FROM dbo.Managers
    WHERE UserId = @UserId;
END;
