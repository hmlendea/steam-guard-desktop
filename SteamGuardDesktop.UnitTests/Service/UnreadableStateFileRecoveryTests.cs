using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class UnreadableStateFileRecoveryTests
    {
        private string filePath = string.Empty;

        [SetUp]
        public void SetUp() => filePath = Path.Combine(
            Path.GetTempPath(),
            "SteamGuardDesktopTests",
            Guid.NewGuid().ToString("N"),
            "state.bin");

        [TearDown]
        public void TearDown()
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Test]
        public void GivenAFilePath_WhenComposingTheQuarantinedPath_ThenTheSuffixIsAppended()
        {
            string quarantinedPath = UnreadableStateFileRecovery.ComposeQuarantinedPath(filePath);

            Assert.That(quarantinedPath, Is.EqualTo(filePath + ".unreadable"));
        }

        [Test]
        public void GivenAWhitespaceFilePath_WhenComposingTheQuarantinedPath_ThenItIsRejected()
        {
            Assert.Throws<ArgumentException>(() => UnreadableStateFileRecovery.ComposeQuarantinedPath(" "));
        }

        [Test]
        public void GivenAnUnreadableFile_WhenQuarantining_ThenTheOriginalPathNoLongerExists()
        {
            WriteFile("unreadable-payload");

            UnreadableStateFileRecovery.Quarantine(filePath);

            Assert.That(File.Exists(filePath), Is.False);
        }

        [Test]
        public void GivenAnUnreadableFile_WhenQuarantining_ThenThePayloadIsPreservedBesideTheOriginal()
        {
            WriteFile("unreadable-payload");

            string? quarantinedPath = UnreadableStateFileRecovery.Quarantine(filePath);

            Assert.That(quarantinedPath, Is.Not.Null);
            Assert.That(File.Exists(quarantinedPath), Is.True);
            Assert.That(File.ReadAllText(quarantinedPath!), Is.EqualTo("unreadable-payload"));
        }

        [Test]
        public void GivenAnUnreadableFileThatWasAlreadyQuarantined_WhenQuarantiningAgain_ThenTheLatestPayloadWins()
        {
            WriteFile("first-payload");
            UnreadableStateFileRecovery.Quarantine(filePath);

            WriteFile("second-payload");
            string? quarantinedPath = UnreadableStateFileRecovery.Quarantine(filePath);

            Assert.That(File.ReadAllText(quarantinedPath!), Is.EqualTo("second-payload"));
        }

        [Test]
        public void GivenAMissingFile_WhenQuarantining_ThenNoQuarantinedFileIsProduced()
        {
            string? quarantinedPath = UnreadableStateFileRecovery.Quarantine(filePath);

            Assert.That(quarantinedPath, Is.Null);
            Assert.That(File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(filePath)), Is.False);
        }

        private void WriteFile(string content)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory is not null)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
    }
}
