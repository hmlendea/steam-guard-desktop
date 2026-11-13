using System;
using System.IO;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Recovers from a protected state file that can no longer be decrypted.
    /// </summary>
    /// <remarks>
    /// A file that was written with a different key, or that was truncated or tampered with, cannot be read. The
    /// file is moved aside so that the application can start with empty state instead of failing at startup.
    /// </remarks>
    public static class UnreadableStateFileRecovery
    {
        private static string QuarantineSuffix => ".unreadable";

        /// <summary>
        /// Composes the path that receives a protected state file that cannot be decrypted.
        /// </summary>
        /// <param name="filePath">The path of the protected state file.</param>
        /// <returns>The path of the quarantined file.</returns>
        public static string ComposeQuarantinedPath(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            return filePath + QuarantineSuffix;
        }

        /// <summary>
        /// Moves a protected state file aside so that the application can start with empty state.
        /// </summary>
        /// <param name="filePath">The path of the protected state file.</param>
        /// <returns>The path of the quarantined file, or <c>null</c> when the file was deleted instead.</returns>
        public static string? Quarantine(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            if (!File.Exists(filePath))
            {
                return null;
            }

            string quarantinedPath = ComposeQuarantinedPath(filePath);

            try
            {
                File.Move(filePath, quarantinedPath, overwrite: true);

                return quarantinedPath;
            }
            catch (IOException)
            {
                File.Delete(filePath);

                return null;
            }
        }
    }
}
