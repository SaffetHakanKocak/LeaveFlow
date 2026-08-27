CREATE OR ALTER PROCEDURE dbo.usp_Consultants_Update
    @ConsultantId uniqueidentifier,
    @FirstName nvarchar(80),
    @LastName nvarchar(80),
    @Email nvarchar(256),
    @EmployeeNumber nvarchar(64) = NULL,
    @Department nvarchar(120) = NULL,
    @StartDate date = NULL,
    @IsActive bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId uniqueidentifier;
    DECLARE @NormalizedEmail nvarchar(256) = UPPER(LTRIM(RTRIM(@Email)));

    SELECT @UserId = UserId FROM dbo.Consultants WHERE Id = @ConsultantId;

    IF @UserId IS NULL
        RETURN;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedEmail = @NormalizedEmail AND Id <> @UserId)
        THROW 51002, 'Email is already in use.', 1;

    UPDATE dbo.Users
    SET
        Email = @Email,
        NormalizedEmail = @NormalizedEmail,
        DisplayName = CONCAT(@FirstName, N' ', @LastName),
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @UserId;

    UPDATE dbo.Consultants
    SET
        FirstName = @FirstName,
        LastName = @LastName,
        EmployeeNumber = @EmployeeNumber,
        Department = @Department,
        StartDate = @StartDate,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @ConsultantId;
END;
