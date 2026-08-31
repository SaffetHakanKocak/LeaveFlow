using System.Text.RegularExpressions;

namespace LeaveFlow.SecurityTests;

public sealed class SecretLeakageGuardTests
{
    [Fact]
    public void SourceAndDatabaseScripts_Should_NotContain_PlaintextPasswords_OrHardCodedSecrets()
    {
        var root = FindRepositoryRoot();
        var files = EnumerateSourceFiles(Path.Combine(root, "src"), "*.cs")
            .Concat(EnumerateSourceFiles(Path.Combine(root, "src"), "*.json"))
            .Concat(EnumerateSourceFiles(Path.Combine(root, "db"), "*.sql"))
            .Concat(EnumerateSourceFiles(Path.Combine(root, "docs"), "*.md"));

        var secretPattern = new Regex(
            @"\bPassword\s*=\s*['""][^'""]+['""]|\bpwd\s*=\s*[^;]+|BEGIN (RSA |OPENSSH )?PRIVATE KEY|""Password""\s*:\s*""[^""]+""|""ApiKey""\s*:\s*""[^""]+""|AI__Azure__ApiKey\s*=\s*\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var matches = files
            .Select(file => new { File = file, Text = File.ReadAllText(file) })
            .Where(file => secretPattern.IsMatch(file.Text))
            .Select(file => Path.GetRelativePath(root, file.File))
            .ToArray();

        Assert.Empty(matches);
    }

    [Fact]
    public void ApplicationAndInfrastructure_Should_NotLog_Passwords_Cookies_OrConnectionStrings()
    {
        var root = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        var leakPattern = new Regex(
            @"Log(Information|Warning|Error|Debug|Critical)\([^;]*\b(PasswordHash|ConnectionString|auth cookie)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var matches = files
            .Where(file => leakPattern.IsMatch(File.ReadAllText(file)))
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
