using System.Xml.Linq;

namespace LeaveFlow.UnitTests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void ProjectReferences_Should_Follow_LayeringRules()
    {
        var root = FindRepositoryRoot();

        var domainReferences = GetProjectReferences(root, "src/LeaveFlow.Domain/LeaveFlow.Domain.csproj");
        var applicationReferences = GetProjectReferences(root, "src/LeaveFlow.Application/LeaveFlow.Application.csproj");
        var infrastructureReferences = GetProjectReferences(root, "src/LeaveFlow.Infrastructure/LeaveFlow.Infrastructure.csproj");

        Assert.Empty(domainReferences);
        Assert.Equal(["src/LeaveFlow.Domain/LeaveFlow.Domain.csproj"], applicationReferences);
        Assert.Equal(
            [
                "src/LeaveFlow.Application/LeaveFlow.Application.csproj",
                "src/LeaveFlow.Domain/LeaveFlow.Domain.csproj"
            ],
            infrastructureReferences);
    }

    [Fact]
    public void Infrastructure_Should_NotReference_HostProjects()
    {
        var root = FindRepositoryRoot();
        var infrastructureReferences = GetProjectReferences(root, "src/LeaveFlow.Infrastructure/LeaveFlow.Infrastructure.csproj");

        Assert.DoesNotContain("src/LeaveFlow.Api/LeaveFlow.Api.csproj", infrastructureReferences);
        Assert.DoesNotContain("src/LeaveFlow.Web/LeaveFlow.Web.csproj", infrastructureReferences);
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

    private static string[] GetProjectReferences(string root, string projectPath)
    {
        var fullPath = Path.Combine(root, projectPath.Replace('/', Path.DirectorySeparatorChar));
        var projectDirectory = Path.GetDirectoryName(fullPath) ?? root;
        var document = XDocument.Load(fullPath);

        return document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>()
            .Select(reference => Path.GetRelativePath(root, Path.GetFullPath(Path.Combine(projectDirectory, reference))))
            .Select(reference => reference.Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
