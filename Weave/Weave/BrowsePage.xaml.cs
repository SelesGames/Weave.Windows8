using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weave.Common;
using Weave.ViewModels;
using Weave.ViewModels.Browse;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Weave
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BrowsePage : Weave.Common.LayoutAwarePage
    {
        public const String NavParamSelectionKey = "Selection";
        public const String NavParamSelectedCategoryKey = "Category";
        public const String NavParamSelectedSourceKey = "Source";

        private NewsFeed _feed = new NewsFeed();
        private NavigationViewModel _nav = new NavigationViewModel();

        private int _initialFeedCount = 20;

        private const int DefaultBrowserWidth = 750;

        private Stack<Uri> _browserBackStack = new Stack<Uri>();

        private String _initialSelectedCategory;
        private Guid? _initialSelectedItemId = null;
        private Guid? _initialSelectedSource = null;

        public BrowsePage()
        {
            this.InitializeComponent();

            _feed.FirstVideoLoaded += FirstVideoLoaded;
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
            this.DataContext = _feed;

            if (navigationParameter != null && navigationParameter is Dictionary<String, object>)
            {
                Dictionary<String, object> parameters = (Dictionary<String, object>)navigationParameter;
                if (parameters.ContainsKey(NavParamSelectedCategoryKey)) _initialSelectedCategory = parameters[NavParamSelectedCategoryKey] as String;
                if (parameters.ContainsKey(NavParamSelectionKey)) _initialSelectedItemId = (Guid)parameters[NavParamSelectionKey];
                if (parameters.ContainsKey(NavParamSelectedSourceKey)) _initialSelectedSource = (Guid)parameters[NavParamSelectedSourceKey];
            }

            _feed.IsLoading = true;
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
            if (e.ClickedItem != null)
            {
                itemGridView.SelectedItem = e.ClickedItem;
                ShowArticle(e.ClickedItem as NewsItem);
            }
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
                    if (newValue > (sv.ScrollableHeight - loadMoreThreshold) && !_feed.IsLoading && _feed.HasNextPage) _feed.LoadNextPage();
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

        private async void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = true;
            int initialSelection = InitNav();
            GrdVwNavigation.DataContext = _nav;
            GrdVwNavigation.SelectedIndex = initialSelection;
        }

        public const int NavSpacerHeight = 20;

        private int InitNav()
        {
            int initialSelection = 0;
            ObservableCollection<object> items = _nav.Items;
            items.Add(new CategoryViewModel() { DisplayName = "Latest News", Type = CategoryViewModel.CategoryType.Latest });
            items.Add(new SpacerViewModel() { Height = NavSpacerHeight });

            String noCategoryKey = "";
            Dictionary<String, List<Feed>> categoryFeeds = UserHelper.Instance.CategoryFeeds;
            if (categoryFeeds != null && categoryFeeds.Count > 0)
            {
                List<String> orderedKeys = new List<string>(categoryFeeds.Keys.OrderBy(s => s));
                foreach (String category in orderedKeys)
                {
                    if (!String.Equals(category, noCategoryKey))
                    {
                        if (_initialSelectedCategory != null && String.Equals(_initialSelectedCategory, category, StringComparison.OrdinalIgnoreCase)) initialSelection = items.Count;

                        items.Add(new CategoryViewModel() { DisplayName = category, Info = new CategoryInfo() { Category = category } });
                        List<Feed> feeds = categoryFeeds[category];
                        feeds.Sort((a,b) => String.Compare(a.Name, b.Name));
                        foreach (Feed feed in feeds)
                        {
                            if (_initialSelectedSource != null && _initialSelectedSource.Value == feed.Id) initialSelection = items.Count;
                            items.Add(feed);
                        }
                        items.Add(new SpacerViewModel() { Height = NavSpacerHeight });
                    }
                }

                if (categoryFeeds.ContainsKey(noCategoryKey))
                {
                    items.Add(new CategoryViewModel() { DisplayName = "Other", Type = CategoryViewModel.CategoryType.Other });
                    List<Feed> feeds = categoryFeeds[noCategoryKey];
                    feeds.Sort((a, b) => String.Compare(a.Name, b.Name));
                    foreach (Feed feed in feeds)
                    {
                        if (_initialSelectedSource != null && initialSelection == 0 &&  _initialSelectedSource.Value == feed.Id) initialSelection = items.Count;
                        items.Add(feed);
                    }
                    items.Add(new SpacerViewModel() { Height = NavSpacerHeight });
                }
            }
            return initialSelection;
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

        private async void GrdVwNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageLoaded && e.AddedItems.Count > 0)
            {
                if (MainScrollViewer != null) MainScrollViewer.ScrollToVerticalOffset(0);
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                _feed.ClearData();
                object selected = GrdVwNavigation.SelectedItem;
                if (selected is Feed)
                {
                    await ProcessFeedSelection((Feed)selected);
                }
                else if (selected is CategoryViewModel)
                {
                    await ProcessCategorySelection((CategoryViewModel)selected);
                }

                if (_initialSelectedItemId != null)
                {
                    NewsItem initialSelection = _feed.FindItemById(_initialSelectedItemId.Value);
                    if (initialSelection != null)
                    {
                        itemGridView.SelectedItem = initialSelection;
                        await Task.Delay(1); // allow rendering of page
                        ShowArticle(initialSelection);
                        _initialSelectedItemId = null;
                    }
                }
            }
        }

        private async Task ProcessFeedSelection(Feed feed)
        {
            _feed.FeedId = feed.Id;
            await _feed.LoadInitialData();
            
        }

        private async Task ProcessCategorySelection(CategoryViewModel category)
        {
            if (category.Type == CategoryViewModel.CategoryType.Latest)
            {
                _feed.IsLoading = true;
                foreach (NewsItem item in UserHelper.Instance.GetLatestNews())
                {
                    _feed.AddItem(item);
                }
                _feed.IsLoading = false;
            }
            else if (category.Type == CategoryViewModel.CategoryType.Other)
            {
            }
            else
            {
                _feed.CategoryName = category.Info.Category;
                await _feed.LoadInitialData();
            }
        }

        private void FirstVideoLoaded(object obj)
        {
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void RectOverlay_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CloseArticle();
        }

        private void ShowArticle(NewsItem item)
        {
            _browserBackStack.Clear();
            BtnBrowserBack.IsEnabled = false;
            if (item != null)
            {
                RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ScrlVwrArticle.DataContext = item;
                ScrlVwrArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                SbArticleFlyIn.Begin();
                ParseArticle(item);
            }
        }

        private void CloseArticle()
        {
            ScrlVwrArticle.DataContext = null;
            RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ScrlVwrArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            WebVwArticle.NavigateToString("");
            GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ScrlVwrArticle.Width = DefaultBrowserWidth;
            //WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //SbArticleFlyOut.Begin();
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.Image image = sender as Windows.UI.Xaml.Controls.Image;
            if (image != null && image.Opacity < 1)
            {
                image.Opacity = 1;
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.Image image = sender as Windows.UI.Xaml.Controls.Image;
            if (image != null && image.Source is BitmapImage)
            {
                BitmapImage bmp = (BitmapImage)image.Source;
                if (bmp.PixelHeight != 0) image.Opacity = 1;
            }
        }

        private void SbArticleFlyOut_Completed(object sender, object e)
        {
            ScrlVwrArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void ParseArticle(NewsItem item)
        {
            if (item != null)
            {
                PrgRngBrowserLoading.IsActive = true;
                bool loadWebBrowser = true;

                if (item.Feed.ArticleViewingType == ArticleViewingType.Mobilizer)
                {
                    loadWebBrowser = false;
                    String content = await MobilizerHelper.GetMobilizedHtml(item);
                    if (content != null)
                    {
                        WebVwArticle.NavigateToString(content);
                    }
                    else
                    {
                        loadWebBrowser = true;
                    }
                }

                if (loadWebBrowser)
                {
                    BrowseToWebPage(item.Link);
                }
            }
        }

        private void BrowseToWebPage(String url)
        {
            PrgRngBrowserLoading.IsActive = true;
            ScrlVwrArticle.Width = 1000;
            WebVwArticle.Navigate(new Uri(url, UriKind.Absolute));
            TxtBxBrowserUrl.Text = url;
            GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void WebVwArticle_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!Uri.Equals(e.Uri, _backUri)) _browserBackStack.Push(e.Uri);
            else _backUri = null;

            if (_browserBackStack.Count > 1) BtnBrowserBack.IsEnabled = true;

            WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
            PrgRngBrowserLoading.IsActive = false;

            if (e.Uri != null && !String.IsNullOrEmpty(e.Uri.OriginalString))
            {
                TxtBxBrowserUrl.Text = e.Uri.OriginalString;
                GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                TxtBxBrowserUrl.Text = "";
                GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void TxtBxBrowserUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtBxBrowserUrl.SelectAll();
        }

        private void TxtBxBrowserUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtBxBrowserUrl.Select(0, 0);
        }

        private Uri _backUri;

        private void BtnBrowserBack_Click(object sender, RoutedEventArgs e)
        {
            if (_browserBackStack.Count > 0)
            {
                _backUri = _browserBackStack.Pop();
                _backUri = _browserBackStack.Peek();
                if (_browserBackStack.Count < 2) BtnBrowserBack.IsEnabled = false;

                WebVwArticle.Navigate(_backUri);
            }
        }

        private async void WebVwArticle_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                Dictionary<String, String> parameters = ParseParameters(e.Value, '&');
                if (parameters.ContainsKey("LaunchLink"))
                {
                    String id = parameters.ContainsKey("Id") ? parameters["Id"] : null;
                    if (!String.Equals(id, "sg_link"))
                    {
                        WebVwArticle.NavigateToString("");
                        await Task.Delay(50);
                        BrowseToWebPage(parameters["LaunchLink"]);
                    }
                    else
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(parameters["LaunchLink"], UriKind.Absolute));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private Dictionary<String, String> ParseParameters(String s, char separator)
        {
            Dictionary<String, String> parameters = new Dictionary<string, string>();
            String[] components;
            foreach (String str in s.Split(separator))
            {
                components = str.Split('=');
                if (components.Length > 1)
                {
                    parameters[components[0]] = components[1];
                }
            }
            return parameters;
        }

        private void NavigationPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 1 && GrdVwNavigation.SelectedIndex > 0)
            {
                GrdVwNavigation.ScrollIntoView(GrdVwNavigation.Items[GrdVwNavigation.SelectedIndex]);
            }
        }

    } // end of class
}
