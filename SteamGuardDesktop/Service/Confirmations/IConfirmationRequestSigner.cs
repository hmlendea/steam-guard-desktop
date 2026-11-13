using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Produces the signature that authorises a Steam confirmation request.
    /// </summary>
    public interface IConfirmationRequestSigner
    {
        /// <summary>
        /// Produces the confirmation signature for the specified moment and request tag.
        /// </summary>
        /// <param name="identitySecret">The Base64 identity secret of the account.</param>
        /// <param name="unixTimeSeconds">The Unix timestamp, in seconds, reported by Steam.</param>
        /// <param name="tag">The request tag, such as <c>conf</c> or <c>details</c>.</param>
        /// <returns>The Base64 signature.</returns>
        string GenerateSignature(string identitySecret, long unixTimeSeconds, string tag);
    }
}
