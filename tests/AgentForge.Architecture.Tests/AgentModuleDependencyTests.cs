using System.Xml.Linq;
using Xunit;

namespace AgentForge.Architecture.Tests;

public sealed class AgentModuleDependencyTests
{
    [Fact]
    public void SimpleReflex_DomainHasNoProjectDependencies()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.SimpleReflex.Domain/AgentForge.Agents.SimpleReflex.Domain.csproj");
    }

    [Fact]
    public void SimpleReflex_ApplicationDependsOnlyOnAbstractionsAndItsDomain()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.SimpleReflex.Application/AgentForge.Agents.SimpleReflex.Application.csproj",
            "src/AgentForge.Abstractions/AgentForge.Abstractions.csproj",
            "src/AgentForge.Agents.SimpleReflex.Domain/AgentForge.Agents.SimpleReflex.Domain.csproj");
    }

    [Fact]
    public void SimpleReflex_PublicModuleComposesOnlyItsOwnLayersAndAbstractions()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.SimpleReflex/AgentForge.Agents.SimpleReflex.csproj",
            "src/AgentForge.Abstractions/AgentForge.Abstractions.csproj",
            "src/AgentForge.Agents.SimpleReflex.Application/AgentForge.Agents.SimpleReflex.Application.csproj",
            "src/AgentForge.Agents.SimpleReflex.Domain/AgentForge.Agents.SimpleReflex.Domain.csproj");
    }

    [Fact]
    public void ModelBasedReflex_DomainHasNoProjectDependencies()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.ModelBasedReflex.Domain/AgentForge.Agents.ModelBasedReflex.Domain.csproj");
    }

    [Fact]
    public void ModelBasedReflex_ApplicationDependsOnlyOnAbstractionsAndItsDomain()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.ModelBasedReflex.Application/AgentForge.Agents.ModelBasedReflex.Application.csproj",
            "src/AgentForge.Abstractions/AgentForge.Abstractions.csproj",
            "src/AgentForge.Agents.ModelBasedReflex.Domain/AgentForge.Agents.ModelBasedReflex.Domain.csproj");
    }

    [Fact]
    public void ModelBasedReflex_PublicModuleComposesOnlyItsOwnLayersAndAbstractions()
    {
        AssertProjectReferences(
            "src/AgentForge.Agents.ModelBasedReflex/AgentForge.Agents.ModelBasedReflex.csproj",
            "src/AgentForge.Abstractions/AgentForge.Abstractions.csproj",
            "src/AgentForge.Agents.ModelBasedReflex.Application/AgentForge.Agents.ModelBasedReflex.Application.csproj",
            "src/AgentForge.Agents.ModelBasedReflex.Domain/AgentForge.Agents.ModelBasedReflex.Domain.csproj");
    }

    private static void AssertProjectReferences(
        string projectRelativePath,
        params string[] expectedReferences)
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var projectDirectory = Path.GetDirectoryName(projectPath)
            ?? throw new InvalidOperationException("The project directory could not be resolved.");

        var document = XDocument.Load(projectPath);
        var actualReferences = document
            .Descendants("ProjectReference")
            .Select(element =>
            {
                var include = element.Attribute("Include")?.Value
                    ?? throw new InvalidOperationException("A ProjectReference is missing its Include attribute.");

                var absoluteReferencePath = Path.GetFullPath(Path.Combine(
                    projectDirectory,
                    include.Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar)));

                return Path.GetRelativePath(repositoryRoot, absoluteReferencePath)
                    .Replace('\\', '/');
            })
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        var expected = expectedReferences
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actualReferences);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AgentForge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate AgentForge.sln from the architecture test output directory.");
    }
}
