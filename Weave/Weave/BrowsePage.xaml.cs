using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weave.Common;
using Weave.ViewModels;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Weave
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BrowsePage : Weave.Common.LayoutAwarePage
    {
        private NewsFeed _feed = new NewsFeed();

        public BrowsePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = sender as GridView;
            if (gridView != null)
            {
                if (gridView.Tag == null && gridView.Visibility == Visibility.Visible)
                {
                    ScrollViewer viewer = MainScrollViewer;
                    if (viewer != null)
                    {
                        gridView.Tag = viewer; // attach the scroll viewer to the list box tag for easy access later
                        //App.RegisterForNotification("HorizontalOffset", viewer, 0, MainScrollChanged);
                        App.RegisterForNotification("VerticalOffset", viewer, 0, MainScrollChangedVertical);
                    }
                }
            }
        }

        ScrollViewer _mainScrollViewer = null;
        private ScrollViewer MainScrollViewer
        {
            get
            {
                if (_mainScrollViewer == null)
                {
                    _mainScrollViewer = App.FindSimpleVisualChild<ScrollViewer>(itemGridView);
                }
                return _mainScrollViewer;
            }
        }

        private const double LoadMoreThresholdFactor = 0.7;

        private void MainScrollChangedVertical(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is double)
            {
                double oldValue = (double)e.OldValue;
                double newValue = (double)e.NewValue;
                ScrollViewer sv = d as ScrollViewer;
                if (sv != null && sv.ScrollableHeight > 0 && newValue > oldValue)
                {
                    double loadMoreThreshold = sv.ViewportHeight * LoadMoreThresholdFactor;
                    if (loadMoreThreshold < 1) loadMoreThreshold = 1;
                    //if (newValue > (sv.ScrollableHeight - loadMoreThreshold) && !_feed.IsLoading) _feed.LoadNextPage();
                }
            }
        }

        private VariableSizedWrapGrid _videosContentPanel = null;

        private void WrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_videosContentPanel == null && sender is VariableSizedWrapGrid)
            {
                _videosContentPanel = (VariableSizedWrapGrid)sender;

                ApplicationViewState state = ApplicationView.Value;
                UpdateItemGridView(state);

                GridView_Loaded(itemGridView, null);
            }
        }

        private bool _pageLoaded = false;

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = true;
            foreach (NewsItem item in TestData.GetNewsFeedSample(20)) _feed.Items.Add(item);
            this.DataContext = _feed;
        }

        private void UpdateMainScrollOrientation(ApplicationViewState viewState)
        {
            if (viewState == Windows.UI.ViewManagement.ApplicationViewState.FullScreenPortrait)
            {
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                MainScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                MainScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                MainScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            }
            else if (viewState == ApplicationViewState.Snapped)
            {
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                MainScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                MainScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                MainScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            }
            else
            {
                //MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                //MainScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                //MainScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                //MainScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
                //if (_videosContentPanel != null) _videosContentPanel.Orientation = Orientation.Vertical;
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                MainScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                MainScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                MainScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            }
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationViewState state = ApplicationView.Value;
            UpdateItemGridView(state);
        }

        private void UpdateItemGridView(ApplicationViewState state)
        {
            if (_pageLoaded)
            {
                if (_videosContentPanel != null)
                {
                    UpdateMainScrollOrientation(state);
                }
            }
        }

    } // end of class
}
