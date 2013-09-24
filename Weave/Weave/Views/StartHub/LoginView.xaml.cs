using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weave.ViewModels.StartHub;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Weave.Views.StartHub
{
    public sealed partial class LoginView : UserControl
    {
        public LoginView()
        {
            this.InitializeComponent();
        }

        private async void Account_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (this.DataContext is StartLoginViewModel && button != null && button.DataContext is StartLoginViewModel.LoginInfo)
            {
                StartLoginViewModel vm = (StartLoginViewModel)this.DataContext;
                StartLoginViewModel.LoginInfo info = (StartLoginViewModel.LoginInfo)button.DataContext;
                await vm.Login(info);
            }
        }
    }
}
