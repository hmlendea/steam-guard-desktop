using System;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Service.Confirmations;

using ConfirmationModel = SteamGuardDesktop.Domain.Confirmations.Confirmation;

namespace SteamGuardDesktop.Service.Mapping
{
    /// <summary>
    /// Provides conversions between Steam response data objects and domain models.
    /// </summary>
    internal static class ConfirmationMappingExtensions
    {
        internal static ConfirmationModel ToServiceModel(this ConfirmationDataObject dataObject) => new()
        {
            Identifier = dataObject.Identifier,
            Nonce = dataObject.Nonce,
            Type = SteamConfirmationType.FromReportedType(dataObject.Type),
            TypeName = dataObject.TypeName,
            CreatorIdentifier = dataObject.CreatorIdentifier,
            Headline = dataObject.Headline,
            Summary = string.Join(", ", dataObject.Summary),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(dataObject.CreationTime)
        };
    }
}
