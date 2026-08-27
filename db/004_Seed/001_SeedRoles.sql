MERGE dbo.Roles AS target
USING
(
    VALUES
        (N'Consultant', N'Can manage personal leave requests.'),
        (N'Manager', N'Can review leave requests for assigned consultants.'),
        (N'Administrator', N'Can manage LeaveFlow system configuration.')
) AS source (Name, Description)
ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET
        Description = source.Description,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (Name, Description)
    VALUES (source.Name, source.Description);
