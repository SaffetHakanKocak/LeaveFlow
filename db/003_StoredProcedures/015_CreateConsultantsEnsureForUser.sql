CREATE OR ALTER PROCEDURE dbo.usp_Consultants_EnsureForUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ConsultantId uniqueidentifier;

    SELECT @ConsultantId = Id
    FROM dbo.Consultants
    WHERE UserId = @UserId;

    IF @ConsultantId IS NULL
    BEGIN
        DECLARE @DisplayName nvarchar(160);
        DECLARE @FirstName nvarchar(80);
        DECLARE @LastName nvarchar(80);
        DECLARE @SpaceIndex int;

        SELECT @DisplayName = NULLIF(LTRIM(RTRIM(DisplayName)), N'')
        FROM dbo.Users
        WHERE Id = @UserId;

        SET @DisplayName = COALESCE(@DisplayName, N'Demo Consultant');
        SET @SpaceIndex = CHARINDEX(N' ', @DisplayName);
        SET @FirstName = LEFT(CASE WHEN @SpaceIndex > 1 THEN LEFT(@DisplayName, @SpaceIndex - 1) ELSE @DisplayName END, 80);
        SET @LastName = LEFT(CASE WHEN @SpaceIndex > 1 THEN LTRIM(SUBSTRING(@DisplayName, @SpaceIndex + 1, 160)) ELSE N'Profile' END, 80);

        INSERT INTO dbo.Consultants (UserId, FirstName, LastName, IsActive)
        VALUES (@UserId, @FirstName, @LastName, 1);

        SELECT @ConsultantId = Id
        FROM dbo.Consultants
        WHERE UserId = @UserId;
    END;

    SELECT @ConsultantId AS Id;
END;
