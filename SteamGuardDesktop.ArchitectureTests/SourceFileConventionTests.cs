using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace SteamGuardDesktop.ArchitectureTests
{
    [TestFixture]
    public sealed class SourceFileConventionTests
    {
        private static IEnumerable<string> ProductionSourceFiles => Directory
            .EnumerateFiles(ArchitecturePaths.ProductionSourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(filePath => !IsUnderGeneratedDirectory(filePath))
            .Where(filePath => !IsGeneratedFile(filePath))
            .OrderBy(filePath => filePath, StringComparer.Ordinal);

        private static IEnumerable<string> ProductionProjectFiles => Directory
            .EnumerateFiles(ArchitecturePaths.ProductionSourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(filePath => !IsUnderGeneratedDirectory(filePath))
            .OrderBy(filePath => filePath, StringComparer.Ordinal);

        [Test]
        public void GivenTheProductionSources_WhenEnumerated_ThenTheyAreAvailable()
        {
            Assert.That(ProductionSourceFiles, Is.Not.Empty);
            Assert.That(ProductionProjectFiles, Is.Not.Empty);
        }

        [Test]
        public void GivenEveryProductionSourceFile_WhenParsed_ThenItDeclaresExactlyOneTopLevelType()
        {
            List<string> failures = [];

            foreach (string filePath in ProductionSourceFiles)
            {
                SyntaxNode syntaxRoot = CSharpSyntaxTree
                    .ParseText(File.ReadAllText(filePath))
                    .GetRoot();

                int declaredTypeCount = syntaxRoot
                    .DescendantNodes()
                    .OfType<MemberDeclarationSyntax>()
                    .Count(member => IsTypeDeclaration(member) && !IsNested(member));

                if (declaredTypeCount != 1)
                {
                    failures.Add($"{filePath} declares {declaredTypeCount} top-level types.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryProductionSourceFile_WhenParsed_ThenItsFileNameMatchesItsTypeName()
        {
            List<string> failures = [];

            foreach (string filePath in ProductionSourceFiles)
            {
                SyntaxNode syntaxRoot = CSharpSyntaxTree
                    .ParseText(File.ReadAllText(filePath))
                    .GetRoot();

                string? typeName = syntaxRoot
                    .DescendantNodes()
                    .OfType<MemberDeclarationSyntax>()
                    .Where(member => IsTypeDeclaration(member) && !IsNested(member))
                    .Select(GetTypeIdentifier)
                    .FirstOrDefault();

                if (typeName is null)
                {
                    continue;
                }

                string fileName = Path.GetFileName(filePath);
                string expectedName = fileName.EndsWith(".axaml.cs", StringComparison.Ordinal)
                    ? fileName[..^".axaml.cs".Length]
                    : Path.GetFileNameWithoutExtension(filePath);

                if (!string.Equals(typeName, expectedName, StringComparison.Ordinal))
                {
                    failures.Add($"{filePath} declares '{typeName}'.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryProductionNamespace_WhenParsed_ThenItIsDeclaredAsABlockNamespace()
        {
            List<string> failures = [];

            foreach (string filePath in ProductionSourceFiles)
            {
                SyntaxNode syntaxRoot = CSharpSyntaxTree
                    .ParseText(File.ReadAllText(filePath))
                    .GetRoot();

                if (syntaxRoot.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Any())
                {
                    failures.Add($"{filePath} uses a file-scoped namespace.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryProductionSourceFile_WhenParsed_ThenItNeitherUsesRegionsNorNullableDirectives()
        {
            List<string> failures = [];

            foreach (string filePath in ProductionSourceFiles)
            {
                string content = File.ReadAllText(filePath);

                if (content.Contains("#region", StringComparison.Ordinal) ||
                    content.Contains("#nullable enable", StringComparison.Ordinal))
                {
                    failures.Add($"{filePath} contains a prohibited directive.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void GivenEveryProductionSourceFile_WhenParsed_ThenItEndsWithAnEmptyLine()
        {
            List<string> failures = [];

            foreach (string filePath in ProductionSourceFiles)
            {
                string content = File.ReadAllText(filePath);

                if (!content.EndsWith(Environment.NewLine, StringComparison.Ordinal) &&
                    !content.EndsWith('\n'))
                {
                    failures.Add($"{filePath} does not end with an empty line.");
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        private static bool IsTypeDeclaration(MemberDeclarationSyntax member) =>
            member is ClassDeclarationSyntax or
                InterfaceDeclarationSyntax or
                StructDeclarationSyntax or
                RecordDeclarationSyntax or
                EnumDeclarationSyntax or
                DelegateDeclarationSyntax;

        private static bool IsNested(MemberDeclarationSyntax member) =>
            member.Parent is TypeDeclarationSyntax;

        private static string GetTypeIdentifier(MemberDeclarationSyntax member)
        {
            if (member is DelegateDeclarationSyntax delegateDeclaration)
            {
                return delegateDeclaration.Identifier.ValueText;
            }

            if (member is BaseTypeDeclarationSyntax typeDeclaration)
            {
                return typeDeclaration.Identifier.ValueText;
            }

            return string.Empty;
        }

        private static bool IsUnderGeneratedDirectory(string filePath)
        {
            string normalisedPath = filePath.Replace('\\', '/');

            return normalisedPath.Contains("/bin/", StringComparison.Ordinal) ||
                normalisedPath.Contains("/obj/", StringComparison.Ordinal);
        }

        private static bool IsGeneratedFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            return fileName.EndsWith(".g.cs", StringComparison.Ordinal) ||
                fileName.EndsWith(".Designer.cs", StringComparison.Ordinal);
        }
    }
}