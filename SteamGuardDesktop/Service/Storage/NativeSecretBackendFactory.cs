using System;
using System.Runtime.InteropServices;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Creates the native credential store backend that matches the current host.
    /// </summary>
    public static class NativeSecretBackendFactory
    {
        /// <summary>
        /// Creates the backend that matches the current operating system.
        /// </summary>
        /// <returns>The matching credential store backend.</returns>
        public static INativeSecretBackend Create()
        {
            if (OperatingSystem.IsWindows())
            {
                return new WindowsCredentialBackend();
            }

            if (OperatingSystem.IsMacOS())
            {
                return new MacOsKeychainBackend();
            }

            if (OperatingSystem.IsLinux())
            {
                return new LinuxSecretServiceBackend();
            }

            return new UnavailableSecretBackend();
        }
    }
}
