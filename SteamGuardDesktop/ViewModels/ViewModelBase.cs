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

        private StatusMessageKind statusMessageKind;

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
        /// Gets or sets the kind of the current status message, which decides how it is presented.
        /// </summary>
        public StatusMessageKind StatusMessageKind
        {
            get => statusMessageKind;
            set
            {
                if (SetProperty(ref statusMessageKind, value))
                {
                    OnPropertyChanged(nameof(IsStatusSuccessful));
                    OnPropertyChanged(nameof(IsStatusFailure));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a status message is available.
        /// </summary>
        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        /// <summary>
        /// Gets a value indicating whether the current status message reports a success.
        /// </summary>
        public bool IsStatusSuccessful =>
            HasStatusMessage && StatusMessageKind == StatusMessageKind.Success;

        /// <summary>
        /// Gets a value indicating whether the current status message reports a failure.
        /// </summary>
        public bool IsStatusFailure =>
            HasStatusMessage && StatusMessageKind == StatusMessageKind.Failure;

        /// <summary>
        /// Reports a non-sensitive failure message to the user.
        /// </summary>
        /// <param name="message">The message to display.</param>
        protected void ReportStatus(string message) => ReportStatus(message, StatusMessageKind.Failure);

        /// <summary>
        /// Reports a non-sensitive status message of the specified kind to the user.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="kind">The kind of the message, which decides how it is presented.</param>
        protected void ReportStatus(string message, StatusMessageKind kind)
        {
            StatusMessageKind = kind;
            StatusMessage = message;
        }

        /// <summary>
        /// Clears the current status message.
        /// </summary>
        protected void ClearStatus()
        {
            StatusMessage = string.Empty;
            StatusMessageKind = StatusMessageKind.None;
        }

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
