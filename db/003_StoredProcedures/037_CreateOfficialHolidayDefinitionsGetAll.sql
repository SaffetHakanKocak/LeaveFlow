CREATE OR ALTER PROCEDURE dbo.usp_OfficialHolidayDefinitions_GetAll
    @Search nvarchar(256) = NULL,
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
        COUNT(dayRows.Id) AS DayCount,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.OfficialHolidayDefinitions AS hd
    LEFT JOIN dbo.OfficialHolidayDays AS dayRows ON dayRows.OfficialHolidayDefinitionId = hd.Id
    WHERE (@Search IS NULL OR hd.Name LIKE N'%' + @Search + N'%')
        AND (@FromDate IS NULL OR hd.EndDate >= @FromDate)
        AND (@ToDate IS NULL OR hd.StartDate <= @ToDate)
    GROUP BY hd.Id, hd.Name, hd.StartDate, hd.EndDate
    ORDER BY hd.StartDate DESC, hd.Name ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
