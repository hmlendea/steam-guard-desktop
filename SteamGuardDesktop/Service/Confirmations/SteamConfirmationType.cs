using System;

using SteamGuardDesktop.Domain.Confirmations;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Maps the numeric confirmation types reported by Steam to domain confirmation types.
    /// </summary>
    public static class SteamConfirmationType
    {
        /// <summary>
        /// Maps a numeric confirmation type reported by Steam to a domain confirmation type.
        /// </summary>
        /// <param name="reportedType">The numeric confirmation type reported by Steam.</param>
        /// <returns>The matching domain confirmation type, or <see cref="ConfirmationType.Unsupported"/>.</returns>
        public static ConfirmationType FromReportedType(int reportedType)
        {
            if (Enum.IsDefined(typeof(ConfirmationType), reportedType))
            {
                return (ConfirmationType)reportedType;
            }

            return ConfirmationType.Unsupported;
        }
    }
}
