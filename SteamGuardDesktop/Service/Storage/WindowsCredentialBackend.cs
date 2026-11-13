using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Provides access to the Windows Credential Manager.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class WindowsCredentialBackend : INativeSecretBackend
    {
        private static string CredentialNamespace => "SteamGuardDesktop";

        private static int CredentialTypeGeneric => 1;

        private static int CredentialPersistLocalMachine => 2;

        private static int ErrorNotFound => 1168;

        private static string ReachabilityProbeEntryName => "ReachabilityProbe";

        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability()
        {
            if (IsStoreReachable())
            {
                return SecretStoreAvailability.Available;
            }

            return SecretStoreAvailability.Unavailable;
        }

        /// <inheritdoc />
        public string? Read(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            if (!CredRead(ComposeTargetName(entryName), CredentialTypeGeneric, 0, out IntPtr credentialPointer))
            {
                return null;
            }

            try
            {
                NativeCredential credential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);

                if (credential.CredentialBlobSize == 0)
                {
                    return null;
                }

                byte[] secretBytes = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, secretBytes, 0, credential.CredentialBlobSize);

                return Encoding.Unicode.GetString(secretBytes);
            }
            finally
            {
                CredFree(credentialPointer);
            }
        }

        /// <inheritdoc />
        public bool Write(string entryName, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            byte[] secretBytes = Encoding.Unicode.GetBytes(value);
            IntPtr targetNamePointer = Marshal.StringToCoTaskMemUni(ComposeTargetName(entryName));
            IntPtr userNamePointer = Marshal.StringToCoTaskMemUni(entryName);
            IntPtr secretPointer = Marshal.AllocHGlobal(secretBytes.Length);

            Marshal.Copy(secretBytes, 0, secretPointer, secretBytes.Length);

            NativeCredential credential = new()
            {
                Type = CredentialTypeGeneric,
                TargetName = targetNamePointer,
                CredentialBlobSize = secretBytes.Length,
                CredentialBlob = secretPointer,
                Persist = CredentialPersistLocalMachine,
                UserName = userNamePointer
            };

            try
            {
                return CredWrite(ref credential, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(secretPointer);
                Marshal.FreeCoTaskMem(userNamePointer);
                Marshal.FreeCoTaskMem(targetNamePointer);
            }
        }

        /// <inheritdoc />
        public bool Delete(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            if (CredDelete(ComposeTargetName(entryName), CredentialTypeGeneric, 0))
            {
                return true;
            }

            return Marshal.GetLastWin32Error() == ErrorNotFound;
        }

        private static string ComposeTargetName(string entryName) => $"{CredentialNamespace}:{entryName}";

        /// <summary>
        /// Determines whether the Credential Manager answers a read for an entry that is never stored, so that a
        /// session without a usable Credential Manager is reported as unavailable.
        /// </summary>
        /// <returns><c>true</c> when the Credential Manager answered the read.</returns>
        private static bool IsStoreReachable()
        {
            bool wasRead = CredRead(
                ComposeTargetName(ReachabilityProbeEntryName),
                CredentialTypeGeneric,
                0,
                out IntPtr credentialPointer);

            if (wasRead)
            {
                CredFree(credentialPointer);

                return true;
            }

            return Marshal.GetLastWin32Error() == ErrorNotFound;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPointer);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite(ref NativeCredential credential, int flags);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredDelete(string target, int type, int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr buffer);

        private struct NativeCredential
        {
            public int Flags;

            public int Type;

            public IntPtr TargetName;

            public IntPtr Comment;

            public long LastWritten;

            public int CredentialBlobSize;

            public IntPtr CredentialBlob;

            public int Persist;

            public int AttributeCount;

            public IntPtr Attributes;

            public IntPtr TargetAlias;

            public IntPtr UserName;
        }
    }
}
