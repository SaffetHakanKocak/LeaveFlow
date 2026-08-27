CREATE OR ALTER PROCEDURE dbo.usp_Consultants_GetAll
    @Search nvarchar(256) = NULL,
    @IsActive bit = NULL,
    @PageNumber int = 1,
    @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 1 THEN 25 WHEN @PageSize > 100 THEN 100 ELSE @PageSize END;

    SELECT
        c.Id,
        c.UserId,
        c.FirstName,
        c.LastName,
        u.Email,
        c.EmployeeNumber,
        c.Department,
        c.IsActive,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.Consultants AS c
    INNER JOIN dbo.Users AS u ON u.Id = c.UserId
    WHERE (@IsActive IS NULL OR c.IsActive = @IsActive)
      AND (
          @Search IS NULL
          OR c.FirstName LIKE N'%' + @Search + N'%'
          OR c.LastName LIKE N'%' + @Search + N'%'
          OR u.Email LIKE N'%' + @Search + N'%'
          OR c.EmployeeNumber LIKE N'%' + @Search + N'%'
      )
    ORDER BY c.LastName, c.FirstName, c.Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
