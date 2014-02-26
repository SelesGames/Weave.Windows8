using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Weave.Common
{
    public class LiveAccountHelper : DependencyObject
    {
        private const string UserNotSignedIn = "You're not signed in.";

        private static LiveAccountHelper _instance = new LiveAccountHelper();
        public static LiveAccountHelper Instance
        {
            get { return _instance; }
        }

        private LiveAccountHelper()
        {
        }

        public static readonly DependencyProperty SignInNameProperty =
            DependencyProperty.Register("SignInName", typeof(string), typeof(LiveAccountHelper),
                                        new PropertyMetadata(UserNotSignedIn));

        public static readonly DependencyProperty IsSignedInProperty =
            DependencyProperty.Register("IsSignedIn", typeof(bool), typeof(LiveAccountHelper),
                                        new PropertyMetadata(false));

        public string SignInName
        {
            get { return (string)this.GetValue(SignInNameProperty); }
            private set { this.SetValue(SignInNameProperty, value); }
        }

        public bool IsSignedIn
        {
            get { return (bool)this.GetValue(IsSignedInProperty); }
            private set { this.SetValue(IsSignedInProperty, value); }
        }

        private LiveAuthClient _authClient;
        private static readonly string[] Scopes = new[] { "wl.signin", "wl.basic" }; //TODO: Add once new scope enabled "Office.OneNote_Create" };

        private bool _loginChecked = false;
        public bool LoginChecked
        {
            get { return _loginChecked; }
        }

        private ManualResetEvent _loginCheckingEvent = new ManualResetEvent(true);

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
            LiveLoginResult loginResult = await AuthClient.InitializeAsync(Scopes);

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
            await this.UpdateAuthProperties(loginResult.Status);
            return loginResult;
        }

        public async Task SignOut()
        {
            LiveLoginResult loginResult = await AuthClient.InitializeAsync(Scopes);

            // Sign the user out, if they are connected
            if (loginResult.Status != LiveConnectSessionStatus.NotConnected)
            {
                AuthClient.Logout();
            }
            await this.UpdateAuthProperties(LiveConnectSessionStatus.NotConnected);
        }

        public async Task<LiveLoginResult> SilentSignIn()
        {
            LiveLoginResult result = null;
            try
            {
                if (_loginCheckingEvent.WaitOne(0))
                {
                    _loginCheckingEvent.Reset();
                    result = await AuthClient.InitializeAsync(Scopes);
                    await this.UpdateAuthProperties(result.Status);
                    _loginChecked = true;
                }
                else
                {
                    _loginCheckingEvent.WaitOne();
                }
            }
            catch (Exception)
            {
            }
            _loginCheckingEvent.Set();
            return result;
        }

        /// <summary>
        /// Update dependency properties that drive the UI for Auth
        /// </summary>
        private async Task UpdateAuthProperties(LiveConnectSessionStatus loginStatus)
        {
            bool signedIn = loginStatus == LiveConnectSessionStatus.Connected;
            this.IsSignedIn = signedIn;
            if (signedIn)
            {
                this.SignInName = "Signed in as " + await this.RetrieveName();
            }
            else
            {
                this.SignInName = UserNotSignedIn;
            }
        }

        /// <summary>
        /// Get the user's name from the profile 
        /// </summary>
        private async Task<string> RetrieveName()
        {
            // Create a client session to get the profile data.
            var lcConnect = new LiveConnectClient(AuthClient.Session);

            // Get the profile info of the user.
            LiveOperationResult operationResult = await lcConnect.GetAsync("me");
            dynamic result = operationResult.Result;
            if (result != null)
            {
                return (string)result.name;
            }
            else
            {
                // Handle the case where the user name was not returned. 
                throw new InvalidOperationException();
            }
        }

    }
}
