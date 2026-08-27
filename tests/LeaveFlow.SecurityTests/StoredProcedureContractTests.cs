namespace LeaveFlow.SecurityTests;

public sealed class StoredProcedureContractTests
{
    [Fact]
    public void InfrastructureRepositories_Should_Call_KnownStoredProcedures_WithStoredProcedureCommandType()
    {
        var root = FindRepositoryRoot();
        var repositoryFiles = Directory.EnumerateFiles(
            Path.Combine(root, "src", "LeaveFlow.Infrastructure", "Persistence", "Repositories"),
            "*.cs",
            SearchOption.AllDirectories);

        foreach (var file in repositoryFiles)
        {
            var text = File.ReadAllText(file);

            Assert.Contains("StoredProcedureNames.", text);
            Assert.Contains("CommandType.StoredProcedure", text);
            Assert.DoesNotContain("CommandType.Text", text);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LeaveFlow.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
