CREATE OR ALTER PROCEDURE dbo.usp_Managers_EnsureForUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ManagerId uniqueidentifier;

    SELECT @ManagerId = Id
    FROM dbo.Managers
    WHERE UserId = @UserId;

    IF @ManagerId IS NULL
    BEGIN
        DECLARE @DisplayName nvarchar(160);
        DECLARE @FirstName nvarchar(80);
        DECLARE @LastName nvarchar(80);
        DECLARE @SpaceIndex int;

        SELECT @DisplayName = NULLIF(LTRIM(RTRIM(DisplayName)), N'')
        FROM dbo.Users
        WHERE Id = @UserId;

        SET @DisplayName = COALESCE(@DisplayName, N'Demo Manager');
        SET @SpaceIndex = CHARINDEX(N' ', @DisplayName);
        SET @FirstName = LEFT(CASE WHEN @SpaceIndex > 1 THEN LEFT(@DisplayName, @SpaceIndex - 1) ELSE @DisplayName END, 80);
        SET @LastName = LEFT(CASE WHEN @SpaceIndex > 1 THEN LTRIM(SUBSTRING(@DisplayName, @SpaceIndex + 1, 160)) ELSE N'Profile' END, 80);

        INSERT INTO dbo.Managers (UserId, FirstName, LastName, IsActive)
        VALUES (@UserId, @FirstName, @LastName, 1);

        SELECT @ManagerId = Id
        FROM dbo.Managers
        WHERE UserId = @UserId;
    END;

    SELECT @ManagerId AS Id;
END;
