CREATE OR ALTER PROCEDURE dbo.usp_Users_Upsert
    @Email nvarchar(256),
    @NormalizedEmail nvarchar(256),
    @DisplayName nvarchar(160),
    @PasswordHash nvarchar(500),
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId uniqueidentifier;

    SELECT @UserId = Id
    FROM dbo.Users
    WHERE NormalizedEmail = @NormalizedEmail;

    IF @UserId IS NULL
    BEGIN
        INSERT INTO dbo.Users
        (
            Email,
            NormalizedEmail,
            DisplayName,
            PasswordHash,
            IsActive
        )
        VALUES
        (
            @Email,
            @NormalizedEmail,
            @DisplayName,
            @PasswordHash,
            @IsActive
        );

        SELECT @UserId = Id
        FROM dbo.Users
        WHERE NormalizedEmail = @NormalizedEmail;
    END
    ELSE
    BEGIN
        UPDATE dbo.Users
        SET
            Email = @Email,
            DisplayName = @DisplayName,
            PasswordHash = @PasswordHash,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @UserId;
    END;

    SELECT @UserId AS Id;
END;
