CREATE OR ALTER PROCEDURE dbo.usp_Consultants_GetIdByUserId
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id
    FROM dbo.Consultants
    WHERE UserId = @UserId;
END;
