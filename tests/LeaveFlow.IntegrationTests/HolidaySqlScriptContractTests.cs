namespace LeaveFlow.IntegrationTests;

public sealed class HolidaySqlScriptContractTests
{
    [Theory]
    [InlineData("033_CreateHolidayDefinitionsCreate.sql", "INSERT INTO dbo.HolidayDays")]
    [InlineData("034_CreateHolidayDefinitionsUpdate.sql", "DELETE FROM dbo.HolidayDays")]
    [InlineData("036_CreateHolidayDefinitionsDelete.sql", "DELETE FROM dbo.HolidayDays")]
    [InlineData("039_CreateOfficialHolidayDefinitionsCreate.sql", "INSERT INTO dbo.OfficialHolidayDays")]
    [InlineData("040_CreateOfficialHolidayDefinitionsUpdate.sql", "DELETE FROM dbo.OfficialHolidayDays")]
    [InlineData("041_CreateOfficialHolidayDefinitionsDelete.sql", "DELETE FROM dbo.OfficialHolidayDays")]
    public void HolidayWriteProcedures_Should_MaintainParentChildRows(string fileName, string expectedSql)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains(expectedSql, sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("033_CreateHolidayDefinitionsCreate.sql")]
    [InlineData("034_CreateHolidayDefinitionsUpdate.sql")]
    [InlineData("039_CreateOfficialHolidayDefinitionsCreate.sql")]
    [InlineData("040_CreateOfficialHolidayDefinitionsUpdate.sql")]
    public void HolidayWriteProcedures_Should_GenerateInclusiveDateRanges(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("WHILE @CurrentDate <= @EndDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DATEADD(day, 1, @CurrentDate)", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("033_CreateHolidayDefinitionsCreate.sql")]
    [InlineData("034_CreateHolidayDefinitionsUpdate.sql")]
    [InlineData("039_CreateOfficialHolidayDefinitionsCreate.sql")]
    [InlineData("040_CreateOfficialHolidayDefinitionsUpdate.sql")]
    public void HolidayWriteProcedures_Should_RejectInvalidRangesAndDuplicates(string fileName)
    {
        var sql = ReadStoredProcedure(fileName);

        Assert.Contains("@StartDate > @EndDate", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("already exists", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadStoredProcedure(string fileName)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(root, "db", "003_StoredProcedures", fileName));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "db")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
