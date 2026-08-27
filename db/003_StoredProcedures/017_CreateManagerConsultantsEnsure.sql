CREATE OR ALTER PROCEDURE dbo.usp_ManagerConsultants_Ensure
    @ManagerId uniqueidentifier,
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.ManagerConsultants
        WHERE ManagerId = @ManagerId
          AND ConsultantId = @ConsultantId
    )
    BEGIN
        INSERT INTO dbo.ManagerConsultants (ManagerId, ConsultantId)
        VALUES (@ManagerId, @ConsultantId);
    END;
END;
