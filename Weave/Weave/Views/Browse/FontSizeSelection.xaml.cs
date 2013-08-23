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
    public sealed partial class FontSizeSelection : UserControl
    {
        public event Action<object, WeaveOptions.FontSize> FontSizeChanged;

        private bool _enableChangeEvent = true;

        public FontSizeSelection()
        {
            this.InitializeComponent();
        }

        private void LstBxFontSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_enableChangeEvent)
            {
                object selected = LstBxFontSizes.SelectedItem;
                WeaveOptions.FontSize size = WeaveOptions.FontSize.Medium;
                if (selected == LstBxItmSmall) size = WeaveOptions.FontSize.Small;
                else if (selected == LstBxItmLarge) size = WeaveOptions.FontSize.Large;
                WeaveOptions.CurrentFontSize = size;
                if (FontSizeChanged != null) FontSizeChanged(this, size);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _enableChangeEvent = false;

            switch (WeaveOptions.CurrentFontSize)
            {
                case WeaveOptions.FontSize.Small:
                    LstBxFontSizes.SelectedItem = LstBxItmSmall;
                    break;
                case WeaveOptions.FontSize.Medium:
                    LstBxFontSizes.SelectedItem = LstBxItmMedium;
                    break;
                case WeaveOptions.FontSize.Large:
                    LstBxFontSizes.SelectedItem = LstBxItmLarge;
                    break;
            }

            _enableChangeEvent = true;
        }
    }
}
