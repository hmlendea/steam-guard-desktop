using System;

using NUnit.Framework;

using SteamGuardDesktop.Service.Login;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class PasswordEncryptorTests
    {
        private static string ModulusBase64 { get; } = ComposePublicKeyMaterial("modulus");

        private static string ModulusHex { get; } = Convert.ToHexString(Convert.FromBase64String(ModulusBase64));

        [TestCase("613")]
        [TestCase("10001")]
        public void GivenAPublicKey_WhenEncryptingAPassword_ThenTheResultIsBase64Encoded(string exponent)
        {
            PasswordEncryptor encryptor = new();

            string encryptedPassword = encryptor.Encrypt(ModulusBase64, exponent, "dummy-password");

            Assert.That(Convert.FromBase64String(encryptedPassword), Is.Not.Empty);
        }

        [Test]
        public void GivenAHexadecimalModulus_WhenEncryptingAPassword_ThenTheResultIsBase64Encoded()
        {
            PasswordEncryptor encryptor = new();

            string encryptedPassword = encryptor.Encrypt(ModulusHex, "10001", "dummy-password");

            Assert.That(Convert.FromBase64String(encryptedPassword), Is.Not.Empty);
        }

        [Test]
        public void GivenTheSameInputs_WhenEncryptingAPassword_ThenTheCipherTextDiffersEachTime()
        {
            PasswordEncryptor encryptor = new();

            string firstResult = encryptor.Encrypt(ModulusBase64, "10001", "dummy-password");
            string secondResult = encryptor.Encrypt(ModulusBase64, "10001", "dummy-password");

            Assert.That(firstResult, Is.Not.EqualTo(secondResult));
        }

        [Test]
        public void GivenAnInvalidModulus_WhenEncryptingAPassword_ThenTheRequestIsRejected()
        {
            PasswordEncryptor encryptor = new();

            Assert.Throws<ArgumentException>(() => encryptor.Encrypt("not-a-key", "10001", "dummy-password"));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GivenNoPassword_WhenEncryptingIt_ThenTheRequestIsRejected(string password)
        {
            PasswordEncryptor encryptor = new();

            Assert.Throws<ArgumentException>(() => encryptor.Encrypt(ModulusBase64, "10001", password));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GivenNoModulus_WhenEncryptingAPassword_ThenTheRequestIsRejected(string modulus)
        {
            PasswordEncryptor encryptor = new();

            Assert.Throws<ArgumentException>(() => encryptor.Encrypt(modulus, "10001", "dummy-password"));
        }

        private static string ComposePublicKeyMaterial(string purpose)
        {
            using System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create(2048);

            return Convert.ToBase64String(rsa.ExportParameters(false).Modulus!);
        }
    }
}
