using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

namespace SteamGuardDesktop.ArchitectureTests
{
    [TestFixture]
    public sealed class ThemeResourceTests
    {
        private static string ResourceFilePath => Path.Combine(
            ArchitecturePaths.ProductionSourceRoot,
            "Views",
            "ThemeResources.axaml");

        private static string[] RequiredResourceKeys =>
        [
            "AppBackgroundBrush",
            "AppForegroundBrush",
            "PanelBackgroundBrush",
            "PanelBorderBrush",
            "SecondaryForegroundBrush",
            "AccentBrush",
            "SuccessBrush",
            "WarningBrush",
            "FailureBrush"
        ];

        private static string[] ThemeMarkupFiles => [.. Directory
            .EnumerateFiles(
                Path.Combine(ArchitecturePaths.ProductionSourceRoot, "Views"),
                "*.axaml",
                SearchOption.AllDirectories)
            .OrderBy(filePath => filePath, StringComparer.Ordinal)];

        [Test]
        public void GivenTheSharedTheme_WhenChecked_ThenItDeclaresEveryRequiredResource()
        {
            XDocument resources = XDocument.Load(ResourceFilePath);

            string[] declaredKeys = [.. resources
                .Root!
                .Elements()
                .Select(element => element.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value ?? string.Empty)];

            Assert.That(declaredKeys, Is.Not.Empty);
            Assert.That(declaredKeys, Is.SupersetOf(RequiredResourceKeys));
        }

        [Test]
        public void GivenTheApplication_WhenChecked_ThenItForcesTheSingleSharedVariant()
        {
            string applicationPath = Path.Combine(ArchitecturePaths.ProductionSourceRoot, "App.axaml");
            string applicationMarkup = File.ReadAllText(applicationPath);

            Assert.That(applicationMarkup, Does.Contain("RequestedThemeVariant=\"Dark\""));
            Assert.That(
                applicationMarkup,
                Does.Contain("avares://SteamGuardDesktop/Views/ThemeResources.axaml"));
            Assert.That(applicationMarkup, Does.Not.Contain("RequestedThemeVariant=\"Default\""));
        }

        [Test]
        public void GivenTheFeatureViews_WhenChecked_ThenTheyUseSharedThemeResources()
        {
            string windowPath = Path.Combine(ArchitecturePaths.ProductionSourceRoot, "Views", "MainWindow.axaml");
            string windowMarkup = File.ReadAllText(windowPath);

            Assert.That(windowMarkup, Does.Not.Contain("Background=\"#"));
            Assert.That(windowMarkup, Does.Not.Contain("Foreground=\"#"));
            Assert.That(windowMarkup, Does.Contain("{DynamicResource AppBackgroundBrush}"));
            Assert.That(windowMarkup, Does.Contain("{DynamicResource AccentBrush}"));
        }

        [Test]
        public void GivenTheThemeMarkup_WhenChecked_ThenNoHostDependentStylingIsReferenced()
        {
            List<string> failures = [];

            foreach (string filePath in ThemeMarkupFiles)
            {
                string markup = File.ReadAllText(filePath);

                if (markup.Contains("Platform=", StringComparison.Ordinal) ||
                    markup.Contains("ActualThemeVariant", StringComparison.Ordinal) ||
                    markup.Contains("Background=\"#", StringComparison.Ordinal) ||
                    markup.Contains("Foreground=\"#", StringComparison.Ordinal))
                {
                    failures.Add(filePath);
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }
    }
}
