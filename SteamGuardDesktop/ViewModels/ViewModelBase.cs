using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Provides change notification for view models.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        private bool isBusy;

        private string statusMessage = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the view model is performing work.
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        /// <summary>
        /// Gets or sets the non-sensitive status message displayed to the user.
        /// </summary>
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (SetProperty(ref statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a status message is available.
        /// </summary>
        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        /// <summary>
        /// Reports a non-sensitive status message to the user.
        /// </summary>
        /// <param name="message">The message to display.</param>
        protected void ReportStatus(string message) => StatusMessage = message;

        /// <summary>
        /// Clears the current status message.
        /// </summary>
        protected void ClearStatus() => StatusMessage = string.Empty;

        /// <summary>
        /// Ensures that the supplied value is not null.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="parameterName">The name of the parameter that supplied the value.</param>
        /// <returns>The validated value.</returns>
        protected static TValue RequireValue<TValue>(TValue value, string parameterName)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);

            return value;
        }
    }
}
