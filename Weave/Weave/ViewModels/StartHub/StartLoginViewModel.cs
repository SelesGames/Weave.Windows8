using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;
using System.Collections.ObjectModel;

namespace Weave.ViewModels.StartHub
{
    public class StartLoginViewModel : StartItemBase
    {
        public class LoginInfo : BindableBase
        {
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

        public ObservableCollection<LoginInfo> Items { get; private set; }

        public String Header { get; set; }

        private enum LoginService { Microsoft, Twitter, Facebook, Google };

        public StartLoginViewModel()
        {
            Items = new ObservableCollection<LoginInfo>();
            Header = "Login & Sync";
            InitLoginItems();
        }

        private void InitLoginItems()
        {
            LoginInfo info;
            info = new LoginInfo();
            info.Header = "Microsoft";
            info.LoggedOutText = "login via Microsoft account";
            info.LoggedOutImage = "";
            info.LoggedInImage = "";
            Items.Add(info);

            info = new LoginInfo();
            info.Header = "Twitter";
            info.LoggedOutText = "login via Twitter account";
            info.LoggedOutImage = "";
            info.LoggedInImage = "";
            Items.Add(info);

            info = new LoginInfo();
            info.Header = "Facebook";
            info.LoggedOutText = "login via Facebook account";
            info.LoggedOutImage = "";
            info.LoggedInImage = "";
            Items.Add(info);

            info = new LoginInfo();
            info.Header = "Google";
            info.LoggedOutText = "login via Google account";
            info.LoggedOutImage = "";
            info.LoggedInImage = "";
            Items.Add(info);
        }
    }
}
