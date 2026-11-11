using System;
using System.Diagnostics;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Provides access to a credential store exposed through a command-line utility.
    /// </summary>
    public abstract class CommandLineSecretBackend : INativeSecretBackend
    {
        /// <summary>
        /// Gets the name of the executable that provides access to the credential store.
        /// </summary>
        protected abstract string ExecutableName { get; }

        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability()
        {
            if (!IsExecutableAvailable())
            {
                return SecretStoreAvailability.Unavailable;
            }

            return SecretStoreAvailability.Available;
        }

        /// <inheritdoc />
        public abstract string? Read(string entryName);

        /// <inheritdoc />
        public abstract bool Write(string entryName, string value);

        /// <inheritdoc />
        public abstract bool Delete(string entryName);

        /// <summary>
        /// Runs the credential store utility with the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the utility.</param>
        /// <param name="standardInput">The standard input to supply, or an empty string.</param>
        /// <returns>The process output, or <c>null</c> when the utility failed.</returns>
        protected ProcessOutput? Run(string arguments, string standardInput)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = ExecutableName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = !string.IsNullOrEmpty(standardInput),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo };

            if (!process.Start())
            {
                return null;
            }

            if (!string.IsNullOrEmpty(standardInput))
            {
                process.StandardInput.Write(standardInput);
                process.StandardInput.Close();
            }

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return new ProcessOutput
            {
                ExitCode = process.ExitCode,
                StandardOutput = standardOutput,
                StandardError = standardError
            };
        }

        private bool IsExecutableAvailable()
        {
            foreach (string directory in GetSearchDirectories())
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(directory, ExecutableName)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] GetSearchDirectories() =>
            [
                "/usr/bin",
                "/usr/local/bin",
                "/bin",
                "/usr/bin/local",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.local/bin"
            ];
    }
}
