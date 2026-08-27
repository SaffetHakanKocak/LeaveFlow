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
        INSERT INTO dbo.Consultants (UserId)
        VALUES (@UserId);

        SELECT @ConsultantId = Id
        FROM dbo.Consultants
        WHERE UserId = @UserId;
    END;

    SELECT @ConsultantId AS Id;
END;
