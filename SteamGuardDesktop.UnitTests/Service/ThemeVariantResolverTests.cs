using NUnit.Framework;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Service.Theming;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class ThemeVariantResolverTests
    {
        [TestCase(ThemePreference.System, "Default")]
        [TestCase(ThemePreference.Light, "Light")]
        [TestCase(ThemePreference.Dark, "Dark")]
        public void GivenAThemePreference_WhenResolvingTheVariant_ThenTheMatchingNameIsProduced(
            ThemePreference themePreference,
            string expectedVariantName)
        {
            ThemeVariantResolver resolver = new();

            Assert.That(resolver.ResolveVariantName(themePreference), Is.EqualTo(expectedVariantName));
        }

        [Test]
        public void GivenAHostThatPrefersDark_WhenFollowingIt_ThenTheDarkPreferenceIsSelected()
        {
            ThemeVariantResolver resolver = new();

            Assert.That(resolver.ResolveFollowedPreference(true), Is.EqualTo(ThemePreference.Dark));
        }

        [Test]
        public void GivenAHostThatPrefersLight_WhenFollowingIt_ThenTheLightPreferenceIsSelected()
        {
            ThemeVariantResolver resolver = new();

            Assert.That(resolver.ResolveFollowedPreference(false), Is.EqualTo(ThemePreference.Light));
        }

        [Test]
        public void GivenAHostChange_WhenReapplyingTheTheme_ThenTheSelectedPreferenceIsPreserved()
        {
            ThemeVariantResolver resolver = new();

            string systemVariant = resolver.ResolveVariantName(ThemePreference.System);
            ThemePreference followedPreference = resolver.ResolveFollowedPreference(true);

            Assert.That(systemVariant, Is.EqualTo("Default"));
            Assert.That(followedPreference, Is.EqualTo(ThemePreference.Dark));
        }
    }
}
