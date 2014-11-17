using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Weave.Common;
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

        Weave.Services.Identity.Client _client;

        private const String ImagePathFormat = "/Assets/Login/{0}.png";

        public StartLoginViewModel()
        {
            Items = new ObservableCollection<LoginInfo>();
            Header = "Login & Sync";
            _client = new Weave.Services.Identity.Client();
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

        private void RefreshLoginStates()
        {
            IdentityInfo identity = UserHelper.Instance.IdentityInfo;
            foreach (LoginInfo info in Items)
            {
                switch (info.Service)
                {
                    case LoginService.Facebook:
                        info.IsLoggedIn = !identity.IsFacebookLoginEnabled;
                        break;
                    case LoginService.Google:
                        info.IsLoggedIn = !identity.IsGoogleLoginEnabled;
                        break;
                    case LoginService.Microsoft:
                        info.IsLoggedIn = !identity.IsMicrosoftLoginEnabled;
                        break;
                    case LoginService.Twitter:
                        info.IsLoggedIn = !identity.IsTwitterLoginEnabled;
                        break;
                    default:
                        break;
                }
            }
        }

        private MobileServiceClient CreateMobileServiceClient()
        {
            return new MobileServiceClient("https://weaveuser.azure-mobile.net/", "AItWGBDhTNmoHYvcCvixuYgxSvcljU97");
        }

        public async Task Login(LoginInfo info)
        {
            bool success = false;

            var viewModel = UserHelper.Instance.IdentityInfo;

            switch (info.Service)
            {
                case LoginService.Facebook:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Facebook, viewModel.SyncFacebook);
                    break;
                case LoginService.Google:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Google, viewModel.SyncGoogle);
                    break;
                case LoginService.Microsoft:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.MicrosoftAccount, viewModel.SyncMicrosoft);;
                    break;
                case LoginService.Twitter:
                    success = await ProcessLogin(MobileServiceAuthenticationProvider.Twitter, viewModel.SyncTwitter);
                    break;
                default:
                    break;
            }
            if (success) RefreshLoginStates();
            IsBusy = false;
        }

        private async Task<bool> ProcessLogin(MobileServiceAuthenticationProvider provider, Func<string, Task> syncFunc)
        {
            bool success = false;

            try
            {
                var mobileUser = await CreateMobileServiceClient().LoginAsync(provider);
                IsBusy = true;
                await syncFunc(mobileUser.UserId);
                success = true;
            }
            catch (Exception ex)
            {
                App.LogError("Error logging into account", ex);
                success = false;
            }

            return success;
        }
    }
}
