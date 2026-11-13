using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;

namespace SteamGuardDesktop.UnitTests.Client
{
    [TestFixture]
    public sealed class ConfirmationDetailsTextConverterTests
    {
        [Test]
        public void GivenAnHtmlFragment_WhenConvertingIt_ThenTheMarkupIsRemoved()
        {
            string html = "<div class=\"trade\"><span>Trade offer</span></div>";

            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.EqualTo("Trade offer"));
        }

        [Test]
        public void GivenAnHtmlFragmentWithAScript_WhenConvertingIt_ThenTheScriptIsRemoved()
        {
            string html = "<div>Item</div><script>alert('secret')</script>";

            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.EqualTo("Item"));
        }

        [Test]
        public void GivenAnHtmlFragmentWithAStyleBlock_WhenConvertingIt_ThenTheStyleIsRemoved()
        {
            string html = "<style>.trade { color: red; }</style><div>Item</div>";

            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.EqualTo("Item"));
        }

        [Test]
        public void GivenAnHtmlFragmentWithEntities_WhenConvertingIt_ThenTheEntitiesAreDecoded()
        {
            string html = "<div>You receive &amp; give</div>";

            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.EqualTo("You receive & give"));
        }

        [Test]
        public void GivenAnHtmlFragmentWithWhitespace_WhenConvertingIt_ThenTheTextIsCollapsed()
        {
            string html = "<div>\n    Item\n    name\n</div>";

            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.EqualTo("Item name"));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GivenNoHtmlFragment_WhenConvertingIt_ThenNothingIsReturned(string html)
        {
            Assert.That(ConfirmationDetailsTextConverter.ToPlainText(html), Is.Empty);
        }
    }
}
