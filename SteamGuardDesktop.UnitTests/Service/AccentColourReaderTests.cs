using System;

using NUnit.Framework;

using SteamGuardDesktop.Service.Theming;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class AccentColourReaderTests
    {
        [TestCase(0x000000FFu, "#FF0000")]
        [TestCase(0x0000FF00u, "#00FF00")]
        [TestCase(0x00FF0000u, "#0000FF")]
        [TestCase(0x00336699u, "#996633")]
        public void GivenAWindowsColourReference_WhenConvertingIt_ThenTheChannelsAreReordered(
            uint colourReference,
            string expectedColour)
        {
            Assert.That(WindowsAccentColourConverter.ToHexString(colourReference), Is.EqualTo(expectedColour));
        }

        [TestCase(0, "#FF5257")]
        [TestCase(4, "#007AFF")]
        [TestCase(7, "#8E8E93")]
        public void GivenAMacOsAccentIdentifier_WhenResolvingIt_ThenTheMatchingColourIsReturned(
            int identifier,
            string expectedColour)
        {
            Assert.That(MacOsAccentColourPalette.FromReportedIdentifier(identifier), Is.EqualTo(expectedColour));
        }

        [TestCase("613")]
        [TestCase("not-a-number")]
        [TestCase("")]
        public void GivenAnUnreadableMacOsAccentValue_WhenResolvingIt_ThenNoColourIsReturned(string reportedValue)
        {
            Assert.That(MacOsAccentColourPalette.Parse(reportedValue), Is.Null);
        }

        [TestCase("blue", "#3584E4")]
        [TestCase("'purple'", "#9141AC")]
        [TestCase("\"teal\"", "#2190A4")]
        public void GivenAGnomeAccentName_WhenResolvingIt_ThenTheMatchingColourIsReturned(
            string accentName,
            string expectedColour)
        {
            Assert.That(GnomeAccentColourPalette.FromAccentName(accentName), Is.EqualTo(expectedColour));
        }

        [TestCase("chartreuse")]
        [TestCase("")]
        public void GivenAnUnknownGnomeAccentName_WhenResolvingIt_ThenNoColourIsReturned(string accentName)
        {
            Assert.That(GnomeAccentColourPalette.FromAccentName(accentName), Is.Null);
        }

        [Test]
        public void GivenAKdeConfiguration_WhenReadingTheAccentColour_ThenTheDeclaredColourIsReturned()
        {
            string configuration = "[General]\nAccentColor=53,132,228\n";

            Assert.That(KdeAccentColourReader.Read(configuration), Is.EqualTo("#3584E4"));
        }

        [Test]
        public void GivenAKdeConfigurationWithoutAnAccentColour_WhenReadingIt_ThenNoColourIsReturned()
        {
            string configuration = "[Colors:Window]\nBackgroundNormal=35,38,41\n";

            Assert.That(KdeAccentColourReader.Read(configuration), Is.Null);
        }

        [Test]
        public void GivenAKdeConfigurationWithAnInvalidAccentColour_WhenReadingIt_ThenNoColourIsReturned()
        {
            string configuration = "[General]\nAccentColor=53,999\n";

            Assert.That(KdeAccentColourReader.Read(configuration), Is.Null);
        }

        [Test]
        public void GivenNoConfiguration_WhenReadingTheAccentColour_ThenNoColourIsReturned()
        {
            Assert.That(KdeAccentColourReader.Read(string.Empty), Is.Null);
        }
    }
}
