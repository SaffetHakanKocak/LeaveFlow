CREATE OR ALTER PROCEDURE dbo.usp_Consultants_Create
    @FirstName nvarchar(80),
    @LastName nvarchar(80),
    @Email nvarchar(256),
    @EmployeeNumber nvarchar(64) = NULL,
    @Department nvarchar(120) = NULL,
    @StartDate date = NULL,
    @IsActive bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NormalizedEmail nvarchar(256) = UPPER(LTRIM(RTRIM(@Email)));
    DECLARE @UserId uniqueidentifier;
    DECLARE @ConsultantRoleId int;

    SELECT @UserId = Id FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail;
    SELECT @ConsultantRoleId = Id FROM dbo.Roles WHERE Name = N'Consultant';

    IF @UserId IS NULL
    BEGIN
        INSERT INTO dbo.Users (Email, NormalizedEmail, DisplayName, IsActive)
        VALUES (@Email, @NormalizedEmail, CONCAT(@FirstName, N' ', @LastName), @IsActive);

        SELECT @UserId = Id FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail;
    END;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Consultants WHERE UserId = @UserId)
    BEGIN
        THROW 51001, 'A consultant already exists for this user.', 1;
    END;

    IF @ConsultantRoleId IS NOT NULL
    BEGIN
        MERGE dbo.UserRoles AS target
        USING (SELECT @UserId AS UserId, @ConsultantRoleId AS RoleId) AS source
        ON target.UserId = source.UserId AND target.RoleId = source.RoleId
        WHEN NOT MATCHED THEN INSERT (UserId, RoleId) VALUES (source.UserId, source.RoleId);
    END;

    INSERT INTO dbo.Consultants
    (
        UserId,
        FirstName,
        LastName,
        EmployeeNumber,
        Department,
        StartDate,
        IsActive
    )
    VALUES
    (
        @UserId,
        @FirstName,
        @LastName,
        @EmployeeNumber,
        @Department,
        @StartDate,
        @IsActive
    );

    SELECT Id FROM dbo.Consultants WHERE UserId = @UserId;
END;
