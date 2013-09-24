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
    public sealed partial class LayoutSizeSelection : UserControl
    {
        public event Action<object, WeaveOptions.LayoutSize> LayoutSizeChanged;

        private bool _enableChangeEvent = true;

        public LayoutSizeSelection()
        {
            this.InitializeComponent();
        }

        private void LstBxLayoutSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_enableChangeEvent)
            {
                object selected = LstBxLayoutSizes.SelectedItem;
                WeaveOptions.LayoutSize size = WeaveOptions.LayoutSize.Normal;
                if (selected == LstBxItmLarge) size = WeaveOptions.LayoutSize.Large;
                WeaveOptions.CurrentLayoutSize = size;
                if (LayoutSizeChanged != null) LayoutSizeChanged(this, size);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _enableChangeEvent = false;

            switch (WeaveOptions.CurrentLayoutSize)
            {
                case WeaveOptions.LayoutSize.Normal:
                    LstBxLayoutSizes.SelectedItem = LstBxItmNormal;
                    break;
                case WeaveOptions.LayoutSize.Large:
                    LstBxLayoutSizes.SelectedItem = LstBxItmLarge;
                    break;
            }

            _enableChangeEvent = true;
        }
    }
}
