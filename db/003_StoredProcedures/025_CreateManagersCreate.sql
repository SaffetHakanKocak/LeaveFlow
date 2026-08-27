CREATE OR ALTER PROCEDURE dbo.usp_Managers_Create
    @FirstName nvarchar(80),
    @LastName nvarchar(80),
    @Email nvarchar(256),
    @Department nvarchar(120) = NULL,
    @IsActive bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NormalizedEmail nvarchar(256) = UPPER(LTRIM(RTRIM(@Email)));
    DECLARE @UserId uniqueidentifier;
    DECLARE @ManagerRoleId int;

    SELECT @UserId = Id FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail;
    SELECT @ManagerRoleId = Id FROM dbo.Roles WHERE Name = N'Manager';

    IF @UserId IS NULL
    BEGIN
        INSERT INTO dbo.Users (Email, NormalizedEmail, DisplayName, IsActive)
        VALUES (@Email, @NormalizedEmail, CONCAT(@FirstName, N' ', @LastName), @IsActive);

        SELECT @UserId = Id FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail;
    END;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Managers WHERE UserId = @UserId)
    BEGIN
        THROW 51003, 'A manager already exists for this user.', 1;
    END;

    IF @ManagerRoleId IS NOT NULL
    BEGIN
        MERGE dbo.UserRoles AS target
        USING (SELECT @UserId AS UserId, @ManagerRoleId AS RoleId) AS source
        ON target.UserId = source.UserId AND target.RoleId = source.RoleId
        WHEN NOT MATCHED THEN INSERT (UserId, RoleId) VALUES (source.UserId, source.RoleId);
    END;

    INSERT INTO dbo.Managers (UserId, FirstName, LastName, Department, IsActive)
    VALUES (@UserId, @FirstName, @LastName, @Department, @IsActive);

    SELECT Id FROM dbo.Managers WHERE UserId = @UserId;
END;
