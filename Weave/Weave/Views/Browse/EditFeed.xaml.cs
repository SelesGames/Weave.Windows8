using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weave.Common;
using Weave.ViewModels;
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
    public sealed partial class EditFeed : UserControl
    {
        public event Action<object, FeedItemViewModel> SaveRequest;

        private FeedItemViewModel _loadedFeed;

        public EditFeed()
        {
            this.InitializeComponent();
        }

        public void LoadFeed(FeedItemViewModel feed)
        {
            if (feed != null)
            {
                _loadedFeed = feed;
                TxtBxName.Text = feed.Feed.Name;
                if (feed.Feed.ArticleViewingType == ArticleViewingType.Mobilizer || feed.Feed.ArticleViewingType == ArticleViewingType.MobilizerOnly) CmbBxArticleView.SelectedItem = CmbBxItmMobilizer;
                else CmbBxArticleView.SelectedItem = CmbBxItmWeb;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtBxName.SelectAll();
            TxtBxName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            String newName = TxtBxName.Text;
            if (!String.IsNullOrEmpty(newName) && _loadedFeed != null)
            {
                Feed model = _loadedFeed.Feed;
                PrgRngSaving.IsActive = true;
                BtnSave.IsEnabled = false;
                BtnSave.Opacity = 0;

                model.Name = newName;
                if (CmbBxArticleView.SelectedItem == CmbBxItmMobilizer) model.ArticleViewingType = ArticleViewingType.Mobilizer;
                else model.ArticleViewingType = ArticleViewingType.InternetExplorer;

                try
                {
                    await UserHelper.Instance.UpdateFeed(model);
                    if (ClusterHelper.EditFeedClusterHeader(model.Id.ToString(), newName)) MainPage.RequireAllRefresh = true;
                }
                catch (Exception)
                {
                }

                BtnSave.IsEnabled = true;
                BtnSave.Opacity = 1;
                PrgRngSaving.IsActive = false;

                if (SaveRequest != null && _loadedFeed != null) SaveRequest(this, _loadedFeed);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadedFeed = null;
        }

        private void TxtBxName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                BtnSave_Click(null, null);
            }
        }
    }
}
