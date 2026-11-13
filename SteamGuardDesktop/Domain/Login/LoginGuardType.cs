namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Identifies the guard that Steam requires before it issues an authenticated session.
    /// </summary>
    public enum LoginGuardType
    {
        /// <summary>
        /// The guard type could not be recognised.
        /// </summary>
        Unsupported = 0,

        /// <summary>
        /// No additional guard is required.
        /// </summary>
        None = 1,

        /// <summary>
        /// A code sent to the registered e-mail address is required.
        /// </summary>
        EmailCode = 2,

        /// <summary>
        /// A code from the mobile authenticator is required.
        /// </summary>
        DeviceCode = 3,

        /// <summary>
        /// The request must be approved on the registered mobile device.
        /// </summary>
        DeviceConfirmation = 4,

        /// <summary>
        /// The sign-in must be approved through the e-mail confirmation link.
        /// </summary>
        EmailConfirmation = 5,

        /// <summary>
        /// A previously issued machine token is required.
        /// </summary>
        MachineToken = 6,

        /// <summary>
        /// A legacy machine authentication code is required.
        /// </summary>
        LegacyMachineAuthentication = 7
    }
}
