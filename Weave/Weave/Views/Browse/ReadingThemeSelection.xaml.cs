using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weave.Common;
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

namespace Weave.Views.Browse
{
    public sealed partial class ReadingThemeSelection : UserControl
    {
        public event Action<object, WeaveOptions.ReadingTheme> ReadingThemeChanged;

        private bool _enableChangeEvent = true;

        public ReadingThemeSelection()
        {
            this.InitializeComponent();
        }

        private void LstBxThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_enableChangeEvent)
            {
                object selected = LstBxThemes.SelectedItem;
                WeaveOptions.ReadingTheme theme = WeaveOptions.ReadingTheme.Light;
                if (selected == LstBxItmDark) theme = WeaveOptions.ReadingTheme.Dark;
                WeaveOptions.CurrentReadingTheme = theme;
                if (ReadingThemeChanged != null) ReadingThemeChanged(this, theme);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _enableChangeEvent = false;

            switch (WeaveOptions.CurrentReadingTheme)
            {
                case WeaveOptions.ReadingTheme.Light:
                    LstBxThemes.SelectedItem = LstBxItmLight;
                    break;
                case WeaveOptions.ReadingTheme.Dark:
                    LstBxThemes.SelectedItem = LstBxItmDark;
                    break;
            }

            _enableChangeEvent = true;
        }
    }
}
