using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Account;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Represents a configurable account entry in the account list.
    /// </summary>
    public sealed class AccountItemViewModel : ObservableObject
    {
        private SteamAccount account;

        /// <summary>
        /// Initialises a new instance of the <see cref="AccountItemViewModel"/> class.
        /// </summary>
        /// <param name="account">The account metadata to display.</param>
        public AccountItemViewModel(SteamAccount account)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));
            this.account = account;
        }

        /// <summary>
        /// Gets the account metadata represented by this entry.
        /// </summary>
        public SteamAccount Account => account;

        /// <summary>
        /// Gets the account identifier.
        /// </summary>
        public string Identifier => account.Identifier;

        /// <summary>
        /// Gets the displayed account label.
        /// </summary>
        public string Label => account.Label;

        /// <summary>
        /// Gets the Steam identifier of the account.
        /// </summary>
        public string SteamIdentifier => account.SteamIdentifier;

        /// <summary>
        /// Updates the account metadata represented by this entry.
        /// </summary>
        /// <param name="updatedAccount">The updated account metadata.</param>
        public void Update(SteamAccount updatedAccount)
        {
            ArgumentNullException.ThrowIfNull(updatedAccount, nameof(updatedAccount));

            account = updatedAccount;

            OnPropertyChanged(nameof(Account));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(SteamIdentifier));
        }
    }
}
