CREATE OR ALTER PROCEDURE dbo.usp_WorkforceTimeline_GetForAdmin
    @StartDate date,
    @EndDate date,
    @ManagerId uniqueidentifier = NULL,
    @ConsultantSearch nvarchar(256) = NULL,
    @IncludeInactive bit = 0,
    @PageNumber int = 1,
    @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset int = (@PageNumber - 1) * @PageSize;

    ;WITH ScopedConsultants AS
    (
        SELECT
            c.Id AS ConsultantId,
            CONCAT(c.FirstName, N' ', c.LastName) AS ConsultantName,
            u.Email AS ConsultantEmail,
            c.IsActive AS ConsultantIsActive
        FROM dbo.Consultants AS c
        INNER JOIN dbo.Users AS u ON u.Id = c.UserId
        WHERE (@IncludeInactive = 1 OR c.IsActive = 1)
            AND (@ConsultantSearch IS NULL
                OR c.FirstName LIKE N'%' + @ConsultantSearch + N'%'
                OR c.LastName LIKE N'%' + @ConsultantSearch + N'%'
                OR u.Email LIKE N'%' + @ConsultantSearch + N'%')
            AND (@ManagerId IS NULL OR EXISTS
            (
                SELECT 1
                FROM dbo.ManagerConsultants AS mc
                WHERE mc.ManagerId = @ManagerId
                    AND mc.ConsultantId = c.Id
            ))
    ),
    CountedConsultants AS
    (
        SELECT
            ConsultantId,
            ConsultantName,
            ConsultantEmail,
            ConsultantIsActive,
            COUNT(1) OVER() AS TotalCount
        FROM ScopedConsultants
    ),
    PagedConsultants AS
    (
        SELECT *
        FROM CountedConsultants
        ORDER BY ConsultantName ASC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    )
    SELECT
        pc.ConsultantId,
        pc.ConsultantName,
        pc.ConsultantEmail,
        pc.ConsultantIsActive,
        cld.LeaveDate,
        cld.LeaveRequestId,
        lr.Reason,
        pc.TotalCount
    FROM PagedConsultants AS pc
    LEFT JOIN dbo.ConsultantLeaveDays AS cld
        ON cld.ConsultantId = pc.ConsultantId
        AND cld.LeaveDate BETWEEN @StartDate AND @EndDate
    LEFT JOIN dbo.LeaveRequests AS lr
        ON lr.Id = cld.LeaveRequestId
        AND lr.Status = N'Approved'
    WHERE cld.Id IS NULL OR lr.Id IS NOT NULL
    ORDER BY pc.ConsultantName ASC, cld.LeaveDate ASC;
END;
