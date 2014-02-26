using Microsoft.Live;
using System;
using System.Threading.Tasks;
using Weave.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Weave
{
    /// <summary>
    /// Reepresents a settings flyout for handlign account sign in/sign out
    /// </summary>
    public sealed partial class OneNoteFlyout : SettingsFlyout
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OneNoteFlyout()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Click handler for sign in button
        /// </summary>
        async void SignInClick(object sender, RoutedEventArgs e)
        {
            BtnSignIn.IsEnabled = false;
            PrgRngBusy.IsActive = true;
            try
            {
                await LiveAccountHelper.Instance.SignIn();
                this.UpdateState();
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
            PrgRngBusy.IsActive = false;
            BtnSignIn.IsEnabled = true;
        }

        /// <summary>
        /// Click handler for sign out button
        /// </summary>
        async void SignOutClick(object sender, RoutedEventArgs e)
        {
            BtnSignOut.IsEnabled = false;
            PrgRngBusy.IsActive = true;
            try
            {
                await LiveAccountHelper.Instance.SignOut();
                this.UpdateState();
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
            PrgRngBusy.IsActive = false;
            BtnSignOut.IsEnabled = true;
        }

        /// <summary>
        /// Update the UI state to match the live status
        /// </summary>
        public void UpdateState()
        {
            try
            {
                bool signedIn = LiveAccountHelper.Instance.IsSignedIn;
                if (signedIn)
                {
                    BtnSignOut.Visibility = (LiveAccountHelper.Instance.AuthClient.CanLogout
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed);
                    BtnSignIn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BtnSignIn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    BtnSignOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LiveAccountHelper.Instance;
            UpdateState();
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            PrgRngBusy.IsActive = false;
        }

    }
}
