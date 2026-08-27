CREATE OR ALTER PROCEDURE dbo.usp_Managers_GetAll
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
        m.Id,
        m.UserId,
        m.FirstName,
        m.LastName,
        u.Email,
        m.Department,
        m.IsActive,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.Managers AS m
    INNER JOIN dbo.Users AS u ON u.Id = m.UserId
    WHERE (@IsActive IS NULL OR m.IsActive = @IsActive)
      AND (
          @Search IS NULL
          OR m.FirstName LIKE N'%' + @Search + N'%'
          OR m.LastName LIKE N'%' + @Search + N'%'
          OR u.Email LIKE N'%' + @Search + N'%'
      )
    ORDER BY m.LastName, m.FirstName, m.Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
