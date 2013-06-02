using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Weave.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Weave.ViewModels.StartHub;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Xml;
using System.Runtime.Serialization.Json;
using Weave.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Weave
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        private static StartHeroArticle _heroArticleVm = new StartHeroArticle();
        private static StartLatestViewModel _latestArticlesVm = new StartLatestViewModel();
        private static StartSourcesViewModel _sourcesVm = new StartSourcesViewModel();
        private static StartAddViewModel _addVm = new StartAddViewModel();

        private const double BaseSectionHeight = 450; // base height of each major section (profile, timeline, photos, etc)
        private static double _savedScrollPosition = 0;

        private static int _defaultStartItemCount; // default item count excluding clusters

        private static ObservableCollection<StartItemBase> _startItems = new ObservableCollection<StartItemBase>();

        public MainPage()
        {
            this.InitializeComponent();

            if (_startItems.Count == 0)
            {
                _startItems.Add(_heroArticleVm);
                _startItems.Add(_latestArticlesVm);
                _startItems.Add(_sourcesVm);
                _startItems.Add(_addVm);

                _defaultStartItemCount = _startItems.Count - 1;
            }
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            PrgRngLoadingMain.IsActive = true;
        }

        private void PopupFlyout_Opened(object sender, object e)
        {
            SbFlyoutPopIn.Begin();
        }

        private void PopupFlyout_Closed(object sender, object e)
        {
        }

        private void semanticZoomControl_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (!e.IsSourceZoomedInView && _semanticItemTapped)
            {
                _semanticItemTapped = false;
                BindableBase sourceItem = e.SourceItem.Item as BindableBase;
                if (sourceItem != null)
                {
                }
            }
        }

        private bool _semanticItemTapped = false;
        private void ZoomedOutItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BindableBase sourceItem = ((FrameworkElement)sender).DataContext as BindableBase;
            if (sourceItem != null)
            {
                //int index = _startItems.IndexOf(sourceItem);
                //if (index <= 1) MainScrollViewer.ScrollToHorizontalOffset(0);
                //else if (index >= _startItems.Count - 2) MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.ScrollableWidth);
                //else LstVwMain.ScrollIntoView(sourceItem);
                //e.Handled = true;
                _semanticItemTapped = true;
            }
        }

        private async Task LoadTestData()
        {
            foreach (String str in TestData.GetSampleSources()) _sourcesVm.Items.Add(new StartSourcesViewModel.SourceListing() { Display = str, Key = str });
        }

        private ScrollViewer _mainScrollViewer;
        private ScrollViewer MainScrollViewer
        {
            get
            {
                if (_mainScrollViewer == null)
                {
                    _mainScrollViewer = App.FindSimpleVisualChild<ScrollViewer>(LstVwMain);
                }
                return _mainScrollViewer;
            }
        }

        private async void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {

            if (_latestArticlesVm.Items.Count == 0)
            {
                await UserHelper.Instance.LoadUser();
                await LoadViewModels();
                await LoadTestData();
            }
            LstVwMain.ItemsSource = _startItems;
            PrgRngLoadingMain.IsActive = false;

            if (_savedScrollPosition > 0)
            {
                await Task.Delay(10); // allow scroll to render
                MainScrollViewer.ScrollToHorizontalOffset(_savedScrollPosition);
                _savedScrollPosition = 0;
            }
        }

        private async Task LoadViewModels()
        {
            List<NewsItem> newsItems = UserHelper.Instance.GetLatestNews();
            if (newsItems != null && newsItems.Count > 0)
            {
                List<StartNewsItemContainer> viewItems = new List<StartNewsItemContainer>();
                int maxCount = StartLatestViewModel.BaseDisplayCount + (StartLatestViewModel.ExtraRows * 3) + 1; // add one for extraction of hero article
                for (int i = 0; i < newsItems.Count && i < maxCount; i++)
                {
                    viewItems.Add(new StartNewsItemContainer(newsItems[i]));
                }
                _heroArticleVm.Article = viewItems[0].NewsItem;
                viewItems.RemoveAt(0);
                _latestArticlesVm.InitItems(viewItems);
            }

            int clusterFetchCount = StartClusterViewModel.BaseDisplayCount + StartClusterViewModel.ExtraRows + 1;
            NewsList news = await UserHelper.Instance.GetCategoryNews("comic books", 0, clusterFetchCount);
            StartClusterViewModel cluster = new StartClusterViewModel();
            cluster.Header = "Comic books";
            _startItems.Insert(_startItems.Count - 1, cluster);
            cluster.InitCluster(news);

            news = await UserHelper.Instance.GetCategoryNews("business", 0, clusterFetchCount);
            cluster = new StartClusterViewModel();
            cluster.Header = "Business";
            _startItems.Insert(_startItems.Count - 1, cluster);
            cluster.InitCluster(news);
        }

        public const double BaseHeight = 768; // base height of design screen
        private const double BinHeight = 280; // extra height to incorporte next bin of results

        private double _expansionFactor;

        private void AdjustForScreenResolution()
        {
            //return;
            if (ApplicationView.Value == ApplicationViewState.FullScreenPortrait)
            {
                _expansionFactor = ((this.ActualWidth - BaseHeight) / BinHeight);
            }
            else
            {
                _expansionFactor = ((this.ActualHeight - BaseHeight) / BinHeight);
            }

            if (_expansionFactor > 0)
            {
                GrdRoot.Tag = BaseSectionHeight + (int)(BinHeight * _expansionFactor);
                StartClusterViewModel.ExtraRows = (int)(_expansionFactor * 2);
                StartLatestViewModel.ExtraRows = (int)_expansionFactor;
            }
            else
            {
                GrdRoot.Tag = BaseSectionHeight;
                StartClusterViewModel.ExtraRows = 0;
                StartLatestViewModel.ExtraRows = 0;
            }
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustForScreenResolution();
        }

    } // end of class
}
