using Microsoft.Live;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Weave
{
    /// <summary>
    /// Reepresents a settings flyout for handlign account sign in/sign out
    /// </summary>
    public sealed partial class OneNoteFlyout : SettingsFlyout
    {
        public static readonly DependencyProperty SignInNameProperty =
            DependencyProperty.Register("SignInName", typeof(string), typeof(OneNoteFlyout),
                                        new PropertyMetadata(null));

        public static readonly DependencyProperty IsSignedInProperty =
            DependencyProperty.Register("IsSignedIn", typeof(bool), typeof(OneNoteFlyout),
                                        new PropertyMetadata(null));
        /// <summary>
        /// Name shown as currently signed in user
        /// </summary>
        public string SignInName
        {
            get { return (string)this.GetValue(SignInNameProperty); }
            set { this.SetValue(SignInNameProperty, value); }
        }

        /// <summary>
        /// Is the user currently signed in
        /// </summary>
        public bool IsSignedIn
        {
            get { return (bool)this.GetValue(IsSignedInProperty); }
            set { this.SetValue(IsSignedInProperty, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OneNoteFlyout()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Opportunity to do flyout-specific logic when it the flyout is opened
        /// </summary>
        //protected override void OnOpening()
        //{
        //    base.OnOpening();
        //    this.UpdateState();
        //}

        /// <summary>
        /// Click handler for sign in button
        /// </summary>
        async void SignInClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //await MainPage.Current.SignIn();
                this.UpdateState();
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
        }

        /// <summary>
        /// Click handler for sign out button
        /// </summary>
        async void SignOutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //await MainPage.Current.SignOut();
                this.UpdateState();
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
        }

        /// <summary>
        /// Update the UI state to match the live status
        /// </summary>
        public void UpdateState()
        {
            try
            {
                //this.SignInName = MainPage.Current.SignInName;
                //this.IsSignedIn = MainPage.Current.IsSignedIn;
                //if (this.IsSignedIn)
                //{
                //    // Show sign-out button if they can sign out.
                //    signOutBtn.Visibility = (MainPage.Current.AuthClient.CanLogout
                //                                 ? Visibility.Visible
                //                                 : Visibility.Collapsed);
                //    signInBtn.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    // Show sign-in button.
                //    signInBtn.Visibility = Visibility.Visible;
                //    signOutBtn.Visibility = Visibility.Collapsed;
                //}
            }
            catch (LiveConnectException)
            {
                // Handle exception.
            }
        }

        private LiveAuthClient _authClient;
        private static readonly string[] Scopes = new[] { "wl.signin", "wl.basic" }; //TODO: Add once new scope enabled "Office.OneNote_Create" };

        /// <summary>
        /// Authentication client to be used across the Page.
        /// </summary>
        public LiveAuthClient AuthClient
        {
            get
            {
                if (_authClient == null)
                {
                    _authClient = new LiveAuthClient();
                }
                return _authClient;
            }
        }

        public async Task<LiveLoginResult> SignIn()
        {
            // First try silent login
            LiveLoginResult loginResult = await AuthClient.InitializeAsync();

            // Sign in to the user's Microsoft account with the required scope.
            //  
            //  This call will display the Microsoft account sign-in screen if 
            //   the user is not already signed in to their Microsoft account 
            //   through Windows 8.
            // 
            //  This call will also display the consent dialog, if the user has 
            //   has not already given consent to this app to access the data 
            //   described by the scope.
            // 
            //  Change the parameter of LoginAsync to include the scopes 
            //   required by your app.
            if (loginResult.Status != LiveConnectSessionStatus.Connected)
            {
                loginResult = await AuthClient.LoginAsync(Scopes);
            }
            this.UpdateAuthProperties(loginResult.Status);
            return loginResult;
        }

        public async Task SignOut()
        {
            LiveLoginResult loginResult = await AuthClient.InitializeAsync();

            // Sign the user out, if they are connected
            if (loginResult.Status != LiveConnectSessionStatus.NotConnected)
            {
                AuthClient.Logout();
            }
            this.UpdateAuthProperties(LiveConnectSessionStatus.NotConnected);
        }

        public async Task<LiveLoginResult> SilentSignIn()
        {
            try
            {
                var loginResult = await AuthClient.InitializeAsync(Scopes);
                var session = loginResult.Session;
                //var token = new global::Common.Microsoft.LiveOfflineAccessToken(loginResult.Session.AccessToken, loginResult.)
                this.UpdateAuthProperties(loginResult.Status);
                return loginResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Update dependency properties that drive the UI for Auth
        /// </summary>
        private async void UpdateAuthProperties(LiveConnectSessionStatus loginStatus)
        {
            bool signedIn = loginStatus == LiveConnectSessionStatus.Connected;
            this.IsSignedIn = signedIn;
            if (signedIn)
            {
                //this.SignInName = await this.RetrieveName();
            }
            else
            {
                //this.SignInName = UserNotSignedIn;
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ApplicationSettings.SettingsPane.Show();
        }
    }
}
