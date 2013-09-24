using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;
using Weave.ViewModels.Identity;

namespace Weave.ViewModels.StartHub
{
    public class StartLoginViewModel : StartItemBase
    {
        public class LoginInfo : BindableBase
        {
            private LoginService _service;
            public LoginService Service
            {
                get { return _service; }
                set { SetProperty(ref _service, value); }
            }

            private String _header;
            public String Header
            {
                get { return _header; }
                set { SetProperty(ref _header, value); }
            }

            private String _loggedOutText;
            public String LoggedOutText
            {
                get { return _loggedOutText; }
                set { SetProperty(ref _loggedOutText, value); }
            }

            private String _loggedInText;
            public String LoggedInText
            {
                get { return _loggedInText; }
                set { SetProperty(ref _loggedInText, value); }
            }

            private bool _isLoggedIn;
            public bool IsLoggedIn
            {
                get { return _isLoggedIn;  }
                set { SetProperty(ref _isLoggedIn, value);}
            }

            private String _loggedInImage;
            public String LoggedInImage
            {
                get { return _loggedInImage; }
                set { SetProperty(ref _loggedInImage, value); }
            }

            private String _loggedOutImage;
            public String LoggedOutImage
            {
                get { return _loggedOutImage; }
                set { SetProperty(ref _loggedOutImage, value); }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public ObservableCollection<LoginInfo> Items { get; private set; }

        public String Header { get; set; }

        public enum LoginService { Microsoft, Twitter, Facebook, Google };

        Weave.Identity.Service.Client.ServiceClient _client;

        private const String ImagePathFormat = "/Assets/Login/{0}.png";

        public StartLoginViewModel()
        {
            Items = new ObservableCollection<LoginInfo>();
            Header = "Login & Sync";
            _client = new Weave.Identity.Service.Client.ServiceClient();
        }

        public async void InitLoginItems()
        {
            if (Items.Count == 0)
            {
                IsBusy = true;
                await UserHelper.Instance.LoadIdentity();

                IdentityInfo identity = UserHelper.Instance.IdentityInfo;
                LoginInfo info;
                info = new LoginInfo();
                info.Service = LoginService.Microsoft;
                info.Header = "Microsoft";
                info.LoggedOutText = "login via Microsoft account";
                info.LoggedInText = "logged in";
                info.LoggedOutImage = String.Format(ImagePathFormat, "Microsoft");
                info.LoggedInImage = String.Format(ImagePathFormat, "MicrosoftOn");
                info.IsLoggedIn = !identity.IsMicrosoftLoginEnabled;
                Items.Add(info);

                info = new LoginInfo();
                info.Service = LoginService.Twitter;
                info.Header = "Twitter";
                info.LoggedOutText = "login via Twitter account";
                info.LoggedInText = "logged in";
                info.LoggedOutImage = String.Format(ImagePathFormat, "Twitter");
                info.LoggedInImage = String.Format(ImagePathFormat, "TwitterOn");
                info.IsLoggedIn = !identity.IsTwitterLoginEnabled;
                Items.Add(info);

                info = new LoginInfo();
                info.Service = LoginService.Facebook;
                info.Header = "Facebook";
                info.LoggedOutText = "login via Facebook account";
                info.LoggedInText = "logged in";
                info.LoggedOutImage = String.Format(ImagePathFormat, "Facebook");
                info.LoggedInImage = String.Format(ImagePathFormat, "FacebookOn");
                info.IsLoggedIn = !identity.IsFacebookLoginEnabled;
                Items.Add(info);

                info = new LoginInfo();
                info.Service = LoginService.Google;
                info.Header = "Google";
                info.LoggedOutText = "login via Google account";
                info.LoggedInText = "logged in";
                info.LoggedOutImage = String.Format(ImagePathFormat, "Google");
                info.LoggedInImage = String.Format(ImagePathFormat, "GoogleOn");
                info.IsLoggedIn = !identity.IsGoogleLoginEnabled;
                Items.Add(info);

                IsBusy = false;
            }
        }

        private MobileServiceClient CreateMobileServiceClient()
        {
            return new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
        }

        public async Task Login(LoginInfo info)
        {
            bool success = false;
            switch (info.Service)
            {
                case LoginService.Facebook:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Facebook, _client.GetUserFromFacebookToken);
                    break;
                case LoginService.Google:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Google, _client.GetUserFromGoogleToken);
                    break;
                case LoginService.Microsoft:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.MicrosoftAccount, _client.GetUserFromMicrosoftToken);;
                    break;
                case LoginService.Twitter:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Twitter, _client.GetUserFromTwitterToken);
                    break;
                default:
                    break;
            }
            if (success) info.IsLoggedIn = true;
            IsBusy = false;
        }

        private async Task LoginFacebook()
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                IsBusy = true;
                Weave.Identity.Service.DTOs.IdentityInfo info = null;
                try
                {
                    info = await _client.GetUserFromFacebookToken(mobileUser.UserId);
                }
                catch (Exception)
                {
                    info = null;
                }

                IdentityInfo viewModel = UserHelper.Instance.IdentityInfo;
                viewModel.FacebookAuthToken = mobileUser.UserId;
                if (info != null)
                {
                    UserHelper.Instance.LoadIdentityDTO(info);
                }
                else
                {
                    await UserHelper.Instance.CreateSyncUserFromCurrent();
                }
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into Facebook account", ex);
            }
        }

        private async Task<bool> ProcessLogin(MobileServiceAuthenticationProvider provider, Func<String, Task<Weave.Identity.Service.DTOs.IdentityInfo>> getUser)
        {
            bool success = false;
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(provider);
                IsBusy = true;
                Weave.Identity.Service.DTOs.IdentityInfo info = null;
                try
                {
                    info = await getUser(mobileUser.UserId);
                }
                catch (Exception)
                {
                    info = null;
                }

                if (info != null)
                {
                    UserHelper.Instance.LoadIdentityDTO(info);
                }
                else
                {
                    IdentityInfo viewModel = UserHelper.Instance.IdentityInfo;

                    switch (provider)
                    {
                        case MobileServiceAuthenticationProvider.Facebook:
                            viewModel.FacebookAuthToken = mobileUser.UserId;
                            break;
                        case MobileServiceAuthenticationProvider.Google:
                            viewModel.GoogleAuthToken = mobileUser.UserId;
                            break;
                        case MobileServiceAuthenticationProvider.MicrosoftAccount:
                            viewModel.MicrosoftAuthToken = mobileUser.UserId;
                            break;
                        case MobileServiceAuthenticationProvider.Twitter:
                            viewModel.TwitterAuthToken = mobileUser.UserId;
                            break;
                        default:
                            return false;
                    }

                    await UserHelper.Instance.CreateSyncUserFromCurrent();
                }
                success = true;
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into Facebook account", ex);
                success = false;
            }
            return success;
        }

        private async Task LoginTwitter()
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                IsBusy = true;
                IdentityInfo viewModel = UserHelper.Instance.IdentityInfo;
                viewModel.TwitterAuthToken = mobileUser.UserId;
                await viewModel.LoadFromTwitter();
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into Twitter account", ex);
            }
        }

        private async Task LoginMicrosoft()
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                IsBusy = true;
                IdentityInfo viewModel = UserHelper.Instance.IdentityInfo;
                viewModel.MicrosoftAuthToken = mobileUser.UserId;
                await viewModel.LoadFromMicrosoft();
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into Microsoft account", ex);
            }
        }

        private async Task LoginGoogle()
        {
            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(MobileServiceAuthenticationProvider.Google);
                IsBusy = true;
                IdentityInfo viewModel = UserHelper.Instance.IdentityInfo;
                viewModel.GoogleAuthToken = mobileUser.UserId;
                await viewModel.LoadFromGoogle();
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into Google account", ex);
            }
        }
    }
}
