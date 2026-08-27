CREATE OR ALTER PROCEDURE dbo.usp_Managers_SetActive
    @ManagerId uniqueidentifier,
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId uniqueidentifier;
    SELECT @UserId = UserId FROM dbo.Managers WHERE Id = @ManagerId;

    UPDATE dbo.Managers
    SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @ManagerId;

    IF @UserId IS NOT NULL
    BEGIN
        UPDATE dbo.Users
        SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @UserId;
    END;
END;
