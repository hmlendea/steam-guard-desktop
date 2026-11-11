using NUnit.Framework;

using SteamGuardDesktop.Service.Confirmations;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class ConfirmationRequestSignerTests
    {
        private static string KnownIdentitySecret => "GQP46b73Ws7gr8GmZFR0sDuau5c=";

        [Test]
        public void GivenKnownFixtureValues_WhenGeneratingASignature_ThenTheKnownSignatureIsProduced()
        {
            ConfirmationRequestSigner signer = new();

            Assert.That(
                signer.GenerateSignature(KnownIdentitySecret, 1617591917L, "conf"),
                Is.EqualTo("NaL8EIMhfy/7vBounJ0CvpKbrPk="));
        }

        [TestCase("conf")]
        [TestCase("details")]
        public void GivenDifferentTags_WhenGeneratingSignatures_ThenTheSignaturesDiffer(string tag)
        {
            ConfirmationRequestSigner signer = new();

            string tagSignature = signer.GenerateSignature(KnownIdentitySecret, 1617591917L, tag);
            string otherSignature = signer.GenerateSignature(KnownIdentitySecret, 1617591917L, tag + "x");

            Assert.That(tagSignature, Is.Not.EqualTo(otherSignature));
        }

        [Test]
        public void GivenDifferentTimestamps_WhenGeneratingSignatures_ThenTheSignaturesDiffer()
        {
            ConfirmationRequestSigner signer = new();

            string firstSignature = signer.GenerateSignature(KnownIdentitySecret, 1617591917L, "conf");
            string secondSignature = signer.GenerateSignature(KnownIdentitySecret, 1617591918L, "conf");

            Assert.That(firstSignature, Is.Not.EqualTo(secondSignature));
        }
    }
}