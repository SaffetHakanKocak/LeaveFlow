using System.Text.RegularExpressions;

namespace LeaveFlow.SecurityTests;

public sealed class DataAccessGuardTests
{
    [Fact]
    public void Source_Should_NotReference_EntityFramework_Or_DbContext()
    {
        var root = FindRepositoryRoot();
        var files = EnumerateSourceFiles(Path.Combine(root, "src"), "*.cs")
            .Concat(EnumerateSourceFiles(Path.Combine(root, "src"), "*.csproj"))
            .Concat(EnumerateSourceFiles(Path.Combine(root, "tests"), "*.csproj"));

        var matches = files
            .Select(file => new { File = file, Text = File.ReadAllText(file) })
            .Where(file => file.Text.Contains("EntityFramework", StringComparison.Ordinal)
                || file.Text.Contains("DbContext", StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(root, file.File))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void ApplicationAndInfrastructure_Should_NotContain_RawSqlStatements_InCSharp()
    {
        var root = FindRepositoryRoot();
        var files = new[]
            {
                Path.Combine(root, "src", "LeaveFlow.Application"),
                Path.Combine(root, "src", "LeaveFlow.Infrastructure")
            }
            .SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        var rawSqlPattern = new Regex(
            @"\b(SELECT\s+\*|SELECT\s+\w+\s+FROM|INSERT\s+INTO|UPDATE\s+\w+\s+SET|DELETE\s+FROM)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var matches = files
            .Where(file =>
            {
                var text = File.ReadAllText(file);
                return rawSqlPattern.IsMatch(text) || text.Contains("CommandType.Text", StringComparison.Ordinal);
            })
            .Select(file => Path.GetRelativePath(root, file))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void WebViews_Should_NotUse_HtmlRawRendering()
    {
        var root = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(
                Path.Combine(root, "src", "LeaveFlow.Web", "Views"),
                "*.cshtml",
                SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        var matches = files
            .Where(file => File.ReadAllText(file).Contains("Html.Raw", StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(root, file))
            .ToArray();

        Assert.Empty(matches);
    }

    private static IEnumerable<string> EnumerateSourceFiles(string path, string pattern)
    {
        return Directory
            .EnumerateFiles(path, pattern, SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
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
