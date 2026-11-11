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
        /// Gets a value indicating whether the confirmation can be approved or denied.
        /// </summary>
        public bool CanBeDecided => confirmation.CanBeDecided;

        /// <summary>
        /// Updates the confirmation represented by this entry.
        /// </summary>
        /// <param name="updatedConfirmation">The updated confirmation.</param>
        public void Update(Confirmation updatedConfirmation)
        {
            ArgumentNullException.ThrowIfNull(updatedConfirmation, nameof(updatedConfirmation));

            confirmation = updatedConfirmation;

            OnPropertyChanged(nameof(Confirmation));
            OnPropertyChanged(nameof(Headline));
            OnPropertyChanged(nameof(TypeName));
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(CreatedAt));
            OnPropertyChanged(nameof(CanBeDecided));
        }
    }
}
