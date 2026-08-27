CREATE OR ALTER PROCEDURE dbo.usp_ManagerConsultants_Assign
    @ManagerId uniqueidentifier,
    @ConsultantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Managers WHERE Id = @ManagerId AND IsActive = 1)
        THROW 51005, 'Manager is not active or does not exist.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Consultants WHERE Id = @ConsultantId AND IsActive = 1)
        THROW 51006, 'Consultant is not active or does not exist.', 1;

    MERGE dbo.ManagerConsultants AS target
    USING (SELECT @ManagerId AS ManagerId, @ConsultantId AS ConsultantId) AS source
    ON target.ManagerId = source.ManagerId AND target.ConsultantId = source.ConsultantId
    WHEN NOT MATCHED THEN INSERT (ManagerId, ConsultantId) VALUES (source.ManagerId, source.ConsultantId);
END;
