CREATE OR ALTER PROCEDURE dbo.usp_Consultants_SetActive
    @ConsultantId uniqueidentifier,
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId uniqueidentifier;
    SELECT @UserId = UserId FROM dbo.Consultants WHERE Id = @ConsultantId;

    UPDATE dbo.Consultants
    SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @ConsultantId;

    IF @UserId IS NOT NULL
    BEGIN
        UPDATE dbo.Users
        SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @UserId;
    END;
END;
