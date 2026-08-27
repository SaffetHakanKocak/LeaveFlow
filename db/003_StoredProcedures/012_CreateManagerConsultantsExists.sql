CREATE OR ALTER PROCEDURE dbo.usp_ManagerConsultants_Exists
    @ManagerId uniqueidentifier,
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.ManagerConsultants
            WHERE ManagerId = @ManagerId
              AND ConsultantId = @ConsultantId
        )
        THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
        END AS IsAssigned;
END;
