using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace SteamGuardDesktop.ArchitectureTests
{
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        private static string ProjectFilePath => Path.Combine(
            ArchitecturePaths.ProductionSourceRoot,
            "SteamGuardDesktop.csproj");

        private static string SolutionFilePath => Path.Combine(
            ArchitecturePaths.ProductionSourceRoot,
            "..",
            "SteamGuardDesktop.slnx");

        private static IReadOnlyList<string> TestProjectNames =>
        [
            "SteamGuardDesktop.ArchitectureTests",
            "SteamGuardDesktop.IntegrationTests",
            "SteamGuardDesktop.UnitTests"
        ];

        [Test]
        public void GivenTheProductionProject_WhenChecked_ThenItTargetsNetTen()
        {
            XDocument project = XDocument.Load(ProjectFilePath);

            Assert.That(
                project.Descendants("TargetFramework").FirstOrDefault()?.Value,
                Is.EqualTo("net10.0"));
        }

        [TestCase("ImplicitUsings", "disable")]
        [TestCase("LangVersion", "latest")]
        [TestCase("IsPackable", "false")]
        [TestCase("Nullable", "enable")]
        public void GivenTheProductionProject_WhenChecked_ThenItDeclaresTheRequiredProperty(
            string propertyName,
            string expectedValue)
        {
            XDocument project = XDocument.Load(ProjectFilePath);

            Assert.That(
                project.Descendants(propertyName).FirstOrDefault()?.Value,
                Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenTheProductionProject_WhenChecked_ThenPackageReferencesAreAlphabetised()
        {
            XDocument project = XDocument.Load(ProjectFilePath);

            string[] packageNames = project
                .Descendants("PackageReference")
                .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty)
                .ToArray();

            Assert.That(packageNames, Is.Not.Empty);
            Assert.That(packageNames, Is.Ordered.Ascending);
        }

        [Test]
        public void GivenTheSolution_WhenChecked_ThenItListsEveryProjectOnce()
        {
            XDocument solution = XDocument.Load(SolutionFilePath);

            string[] projectPaths = solution
                .Descendants("Project")
                .Select(project => project.Attribute("Path")?.Value ?? string.Empty)
                .ToArray();

            Assert.That(projectPaths, Has.Length.EqualTo(4));
            Assert.That(projectPaths.Distinct(StringComparer.Ordinal), Has.Exactly(4).Items);
            Assert.That(projectPaths.First(), Is.EqualTo("SteamGuardDesktop/SteamGuardDesktop.csproj"));
        }

        [Test]
        public void GivenTheSolution_WhenChecked_ThenTheMainProjectComesFirstAndTestsAreAlphabetised()
        {
            XDocument solution = XDocument.Load(SolutionFilePath);

            string[] projectPaths = solution
                .Descendants("Project")
                .Select(project => project.Attribute("Path")?.Value ?? string.Empty)
                .ToArray();

            Assert.That(projectPaths[0], Is.EqualTo("SteamGuardDesktop/SteamGuardDesktop.csproj"));
            Assert.That(projectPaths.Skip(1), Is.Ordered.Ascending);
        }

        [Test]
        public void GivenTheTestProjects_WhenChecked_ThenTheyAreMarkedAsTestProjects()
        {
            foreach (string testProjectName in TestProjectNames)
            {
                XDocument project = XDocument.Load(Path.Combine(
                    ArchitecturePaths.ProductionSourceRoot,
                    "..",
                    testProjectName,
                    testProjectName + ".csproj"));

                Assert.That(
                    project.Descendants("IsTestProject").FirstOrDefault()?.Value,
                    Is.EqualTo("true"),
                    $"{testProjectName} must declare IsTestProject.");
                Assert.That(
                    project.Descendants("ExcludeFromCodeCoverage").FirstOrDefault()?.Value,
                    Is.EqualTo("true"),
                    $"{testProjectName} must declare ExcludeFromCodeCoverage.");
            }
        }
    }
}