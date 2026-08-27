CREATE OR ALTER PROCEDURE dbo.usp_HolidayDefinitions_GetAll
    @Search nvarchar(256) = NULL,
    @IsActive bit = NULL,
    @FromDate date = NULL,
    @ToDate date = NULL,
    @PageNumber int = 1,
    @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset int = (@PageNumber - 1) * @PageSize;

    SELECT
        hd.Id,
        hd.Name,
        hd.StartDate,
        hd.EndDate,
        hd.IsActive,
        COUNT(dayRows.Id) AS DayCount,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.HolidayDefinitions AS hd
    LEFT JOIN dbo.HolidayDays AS dayRows ON dayRows.HolidayDefinitionId = hd.Id
    WHERE (@Search IS NULL OR hd.Name LIKE N'%' + @Search + N'%')
        AND (@IsActive IS NULL OR hd.IsActive = @IsActive)
        AND (@FromDate IS NULL OR hd.EndDate >= @FromDate)
        AND (@ToDate IS NULL OR hd.StartDate <= @ToDate)
    GROUP BY hd.Id, hd.Name, hd.StartDate, hd.EndDate, hd.IsActive
    ORDER BY hd.StartDate DESC, hd.Name ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
