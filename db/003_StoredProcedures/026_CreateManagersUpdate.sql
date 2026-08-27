CREATE OR ALTER PROCEDURE dbo.usp_Managers_Update
    @ManagerId uniqueidentifier,
    @FirstName nvarchar(80),
    @LastName nvarchar(80),
    @Email nvarchar(256),
    @Department nvarchar(120) = NULL,
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId uniqueidentifier;
    DECLARE @NormalizedEmail nvarchar(256) = UPPER(LTRIM(RTRIM(@Email)));

    SELECT @UserId = UserId FROM dbo.Managers WHERE Id = @ManagerId;

    IF @UserId IS NULL
        RETURN;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail AND Id <> @UserId)
        THROW 51004, 'Email is already in use.', 1;

    UPDATE dbo.Users
    SET
        Email = @Email,
        NormalizedEmail = @NormalizedEmail,
        DisplayName = CONCAT(@FirstName, N' ', @LastName),
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @UserId;

    UPDATE dbo.Managers
    SET
        FirstName = @FirstName,
        LastName = @LastName,
        Department = @Department,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @ManagerId;
END;
