CREATE OR ALTER PROCEDURE dbo.usp_ManagerConsultants_Remove
    @ManagerId uniqueidentifier,
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.ManagerConsultants
    WHERE ManagerId = @ManagerId
      AND ConsultantId = @ConsultantId;
END;
