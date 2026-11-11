using System;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Confirmations;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Represents a pending confirmation in the confirmation list.
    /// </summary>
    public sealed class ConfirmationItemViewModel : ObservableObject
    {
        private Confirmation confirmation;

        /// <summary>
        /// Initialises a new instance of the <see cref="ConfirmationItemViewModel"/> class.
        /// </summary>
        /// <param name="confirmation">The confirmation to display.</param>
        public ConfirmationItemViewModel(Confirmation confirmation)
        {
            ArgumentNullException.ThrowIfNull(confirmation, nameof(confirmation));
            this.confirmation = confirmation;
        }

        /// <summary>
        /// Gets the confirmation represented by this entry.
        /// </summary>
        public Confirmation Confirmation => confirmation;

        /// <summary>
        /// Gets the confirmation headline.
        /// </summary>
        public string Headline => confirmation.Headline;

        /// <summary>
        /// Gets the confirmation type name.
        /// </summary>
        public string TypeName => confirmation.TypeName;

        /// <summary>
        /// Gets the additional confirmation context.
        /// </summary>
        public string Summary => confirmation.Summary;

        /// <summary>
        /// Gets the creation time of the confirmation.
        /// </summary>
        public DateTimeOffset CreatedAt => confirmation.CreatedAt;

        /// <summary>
        /// Gets the creation time of the confirmation formatted for display.
        /// </summary>
        public string CreatedAtDisplay => confirmation.CreatedAt.ToLocalTime().ToString(
            "yyyy'-'MM'-'dd' 'HH':'mm",
            System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets a value indicating whether the confirmation can be approved or denied.
        /// </summary>
        public bool CanBeDecided => confirmation.CanBeDecided;
    }
}
