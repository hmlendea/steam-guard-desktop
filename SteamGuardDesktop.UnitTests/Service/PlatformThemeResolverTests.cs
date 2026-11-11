using NUnit.Framework;

using SteamGuardDesktop.Service.Theming;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class PlatformThemeResolverTests
    {
        [Test]
        public void GivenWindows_WhenResolvingTheTheme_ThenTheWindowsFluentThemeIsSelected()
        {
            PlatformThemeResolver resolver = new();

            PlatformTheme theme = resolver.Resolve(new PlatformDescription { IsWindows = true });

            Assert.That(theme, Is.EqualTo(PlatformTheme.WindowsFluent));
        }

        [Test]
        public void GivenMacOs_WhenResolvingTheTheme_ThenTheMacOsAquaThemeIsSelected()
        {
            PlatformThemeResolver resolver = new();

            PlatformTheme theme = resolver.Resolve(new PlatformDescription { IsMacOs = true });

            Assert.That(theme, Is.EqualTo(PlatformTheme.MacOsAqua));
        }

        [Test]
        public void GivenGnome_WhenResolvingTheTheme_ThenTheGnomeAdwaitaThemeIsSelected()
        {
            PlatformThemeResolver resolver = new();

            PlatformTheme theme = resolver.Resolve(new PlatformDescription
            {
                IsLinux = true,
                CurrentDesktop = "GNOME"
            });

            Assert.That(theme, Is.EqualTo(PlatformTheme.GnomeAdwaita));
        }

        [Test]
        public void GivenKde_WhenResolvingTheTheme_ThenTheKdeBreezeThemeIsSelected()
        {
            PlatformThemeResolver resolver = new();

            PlatformTheme theme = resolver.Resolve(new PlatformDescription
            {
                IsLinux = true,
                DesktopEnvironment = "KDE"
            });

            Assert.That(theme, Is.EqualTo(PlatformTheme.KdeBreeze));
        }

        [TestCase("")]
        [TestCase("XFCE")]
        public void GivenAnUnidentifiedDesktopEnvironment_WhenResolvingTheTheme_ThenTheFallbackThemeIsSelected(
            string desktopEnvironment)
        {
            PlatformThemeResolver resolver = new();

            PlatformTheme theme = resolver.Resolve(new PlatformDescription
            {
                IsLinux = true,
                CurrentDesktop = desktopEnvironment,
                DesktopEnvironment = desktopEnvironment
            });

            Assert.That(theme, Is.EqualTo(PlatformTheme.Fallback));
        }

        [Test]
        public void GivenNoKnownPlatform_WhenResolvingTheTheme_ThenTheFallbackThemeIsSelected()
        {
            PlatformThemeResolver resolver = new();

            Assert.That(resolver.Resolve(new PlatformDescription()), Is.EqualTo(PlatformTheme.Fallback));
        }
    }
}
