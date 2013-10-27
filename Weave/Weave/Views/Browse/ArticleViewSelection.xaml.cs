using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class ArticleViewSelection : UserControl
    {
        public event Action<object, bool> ArticleViewChanged;

        private bool _enableChangeEvent = true;

        public ArticleViewSelection()
        {
            this.InitializeComponent();
        }

        private void LstBxViews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_enableChangeEvent && ArticleViewChanged != null) ArticleViewChanged(this, LstBxViews.SelectedItem == LstBxItmMobilizer);
        }

        public void SetArticleView(bool useMobilizer)
        {
            _enableChangeEvent = false;
            if (_canMobilize && useMobilizer) LstBxViews.SelectedItem = LstBxItmMobilizer;
            else LstBxViews.SelectedItem = LstBxItmBrowser;
            _enableChangeEvent = true;
        }

        private bool _canMobilize = true;
        public bool CanMobilize
        {
            set
            {
                _canMobilize = value;
                if (!value) LstBxItmMobilizer.IsEnabled = false;
            }
        }

    }
}
