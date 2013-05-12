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

        private static ObservableCollection<Object> _startItems = new ObservableCollection<Object>();

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
            await Task.Run(() =>
            {
                _heroArticleVm.Article = TestData.GetHeroItemSample();
                List<StartNewsItem> sampleItems = TestData.GetLatestArticlesSample();
                foreach (StartNewsItem item in sampleItems) _latestArticlesVm.Items.Add(item);
                foreach (String str in TestData.GetSampleSources()) _sourcesVm.Items.Add(new StartSourcesViewModel.SourceListing() { Display = str, Key = str });

                StartClusterViewModel cluster = new StartClusterViewModel();
                cluster.Header = "World news";
                _startItems.Insert(_startItems.Count - 1, cluster);
                foreach (StartNewsItem item in TestData.GetWorldNewsSample()) cluster.Items.Add(item);

                cluster = new StartClusterViewModel();
                cluster.Header = "Business";
                _startItems.Insert(_startItems.Count - 1, cluster);
                foreach (StartNewsItem item in TestData.GetBusinessSample()) cluster.Items.Add(item);
            });
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

        private async void LayoutAwarePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTestData();
            LstVwMain.ItemsSource = _startItems;
            PrgRngLoadingMain.IsActive = false;

            if (_savedScrollPosition > 0)
            {
                await Task.Delay(10); // allow scroll to render
                MainScrollViewer.ScrollToHorizontalOffset(_savedScrollPosition);
                _savedScrollPosition = 0;
            }
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
            }
            else
            {
                GrdRoot.Tag = BaseSectionHeight;
            }
        }

        private void LayoutAwarePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustForScreenResolution();
        }

    } // end of class
}
