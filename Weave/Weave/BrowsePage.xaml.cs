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
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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
        private FeedManagementViewModel _feedManageVm = new FeedManagementViewModel();

        private int _initialFeedCount = 20;

        private const int DefaultBrowserWidth = 817;
        private int _browserDisplayWidth = DefaultBrowserWidth;

        private Stack<Uri> _browserBackStack = new Stack<Uri>();

        private Guid? _initialSelectedItemId = null;

        private DispatcherTimer _readTimer;
        private const double ReadInterval = 3;

        private bool _navigatingAway = false;

        private WebViewBrush _browserBrush = new WebViewBrush();

        private bool _showAppBarOnSelection = true;

        public BrowsePage()
        {
            this.InitializeComponent();

            _feed.FirstVideoLoaded += FirstVideoLoaded;

            _readTimer = new DispatcherTimer();
            _readTimer.Interval = TimeSpan.FromSeconds(ReadInterval);
            _readTimer.Tick += ReadTimer_Tick;

            FeedManagementControl.DataContext = _feedManageVm;
            _feedManageVm.FeedAdded += FeedManageVm_FeedAdded;

            _browserBrush.SourceName = "WebVwArticle";
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

            _navigatingAway = false;

            if (navigationParameter != null && navigationParameter is Dictionary<String, object>)
            {
                Dictionary<String, object> parameters = (Dictionary<String, object>)navigationParameter;
                if (parameters.ContainsKey(NavParamSelectedCategoryKey)) _nav.InitialSelectedCategory = parameters[NavParamSelectedCategoryKey] as String;
                if (parameters.ContainsKey(NavParamSelectionKey)) _initialSelectedItemId = (Guid)parameters[NavParamSelectionKey];
                if (parameters.ContainsKey(NavParamSelectedSourceKey)) _nav.InitialSelectedFeed = (Guid)parameters[NavParamSelectedSourceKey];
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
                _showAppBarOnSelection = false;
                itemGridView.SelectedItem = e.ClickedItem;
                _showAppBarOnSelection = true;
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
            int initialSelection = _nav.Initialise();
            GrdVwNavigation.DataContext = _nav;
            GrdVwNavigation.SelectedIndex = initialSelection;
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
                await ProcessSelectedNav();

                if (_initialSelectedItemId != null)
                {
                    NewsItem initialSelection = _feed.FindItemById(_initialSelectedItemId.Value);
                    if (initialSelection != null)
                    {
                        _showAppBarOnSelection = false;
                        itemGridView.SelectedItem = initialSelection;
                        _showAppBarOnSelection = true;
                        await Task.Delay(1); // allow rendering of page
                        ShowArticle(initialSelection, false);
                        _initialSelectedItemId = null;
                    }
                }
            }
        }

        private async Task ProcessSelectedNav(bool refresh = false)
        {
            if (MainScrollViewer != null) MainScrollViewer.ScrollToVerticalOffset(0);
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _feed.ClearData();
            object selected = GrdVwNavigation.SelectedItem;
            if (selected is FeedItemViewModel)
            {
                await ProcessFeedSelection((FeedItemViewModel)selected, refresh);
            }
            else if (selected is CategoryViewModel)
            {
                await ProcessCategorySelection((CategoryViewModel)selected, refresh);
            }
        }

        private async Task ProcessFeedSelection(FeedItemViewModel feed, bool refresh = false)
        {
            _nav.ClearFeedNewCount(feed);
            _feed.SetFeedParam(NewsFeed.FeedType.Feed, feed.Feed.Id);
            if (feed.RequiresRefresh)
            {
                refresh = true;
                feed.RequiresRefresh = false;
            }
            await _feed.LoadInitialData(refresh ? EntryType.ExtendRefresh : EntryType.Mark);
            
        }

        private async Task ProcessCategorySelection(CategoryViewModel category, bool refresh = false)
        {
            EntryType entry = refresh ? EntryType.ExtendRefresh : EntryType.Mark;
            if (category.Type == CategoryViewModel.CategoryType.Latest)
            {
                _feed.IsLoading = true;
                foreach (NewsItem item in UserHelper.Instance.GetLatestNews())
                {
                    _feed.AddItem(item);
                }
                _feed.IsLoading = false;
            }
            else if (category.Type == CategoryViewModel.CategoryType.Favorites)
            {
                _feed.SetFeedParam(NewsFeed.FeedType.Favorites, null);
                await _feed.LoadInitialData(entry);
            }
            else if (category.Type == CategoryViewModel.CategoryType.Other)
            {
            }
            else
            {
                _nav.ClearCategoryNewCount(category);
                _feed.SetFeedParam(NewsFeed.FeedType.Category, category.Info.Category);
                if (category.Type == CategoryViewModel.CategoryType.All) entry = EntryType.Peek;
                else if (category.RequiresRefresh)
                {
                    category.RequiresRefresh = false;
                    entry = EntryType.ExtendRefresh;
                }
                await _feed.LoadInitialData(entry);
            }
        }

        private void FirstVideoLoaded(object obj)
        {
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public static int GetFontSize()
        {
            double scale = Windows.Graphics.Display.DisplayProperties.LogicalDpi / 96;
            return (int)((int)WeaveOptions.CurrentFontSize * scale);
        }

        public static int GetArticleWidth(int fontSize)
        {
            return (int)(fontSize * 41);
        }

        private void ShowArticle(NewsItem item, bool showLoading = true)
        {
            _browserBackStack.Clear();
            BtnBrowserBack.IsEnabled = false;
            if (item != null)
            {
                int fontSize = GetFontSize();
                int articleWidth = GetArticleWidth(fontSize);
                AdjustArticleViewWidth(articleWidth + 160);
                RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ScrlVwrArticle.DataContext = item;
                ScrlVwrArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                PrgRngArticleLoading.IsActive = true;
                if (showLoading) WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                SbArticleFlyIn.Begin();
                ParseArticle(item, fontSize, articleWidth);
            }
        }

        private void CloseArticle()
        {
            _readTimer.Stop();
            SbArticleFlyOut.Begin();
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
            ScrlVwrArticle.DataContext = null;
            RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ScrlVwrArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            WebVwArticle.NavigateToString("");
            GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ScrlVwrArticle.Width = _browserDisplayWidth;
        }

        private async void ParseArticle(NewsItem item, int fontSize, int articleWidth)
        {
            if (item != null)
            {
                PrgRngBrowserLoading.IsActive = true;
                bool loadWebBrowser = true;

                if (item.Feed.ArticleViewingType == ArticleViewingType.Mobilizer)
                {
                    loadWebBrowser = false;
                    String result = await MobilizerHelper.GetMobilizedHtml(item, fontSize, articleWidth);
                    if (result != null)
                    {
                        WebVwArticle.NavigateToString(result);
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
                PrgRngArticleLoading.IsActive = false;
                //WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            _readTimer.Start();
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

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (RectOverlay.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                CloseArticle();
            }
            else base.GoBack(sender, e);
        }

        private void RectOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseArticle();
        }

        private async void ReadTimer_Tick(object sender, object e)
        {
            _readTimer.Stop();
            NewsItem item = itemGridView.SelectedItem as NewsItem;
            if (item != null && !item.HasBeenViewed)
            {
                await UserHelper.Instance.MarkAsRead(item);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _navigatingAway = true;
            _readTimer.Stop();
            base.OnNavigatingFrom(e);
        }

        private async void AppBarRefresh_Click(object sender, RoutedEventArgs e)
        {
            await ProcessSelectedNav(true);
        }

        private void ButtonGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null && grid.Tag == null)
            {
                App.RegisterForNotification("Visibility", grid, Visibility.Collapsed, ButtonGrid_VisibilityChanged);
                grid.Tag = true;
            }
        }

        private void ButtonGrid_VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible.Equals(e.NewValue))
            {
                Grid grid = d as Grid;
                if (grid != null)
                {
                    try
                    {
                        Storyboard sb = grid.Resources["SbShowAnimation"] as Storyboard;
                        sb.Begin();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private async void AppBarFavorite_Click(object sender, RoutedEventArgs e)
        {
            NewsItem item = itemGridView.SelectedItem as NewsItem;
            if (item != null)
            {
                AppBarFavorite.IsEnabled = false;

                await UserHelper.Instance.AddFavorite(item);

                if (!_navigatingAway)
                {
                    AppBarFavorite.IsEnabled = true;
                    AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private async void AppBarUnfavorite_Click(object sender, RoutedEventArgs e)
        {
            NewsItem item = itemGridView.SelectedItem as NewsItem;
            if (item != null)
            {
                AppBarUnfavorite.IsEnabled = false;

                await UserHelper.Instance.RemoveFavorite(item);
                if (_feed != null && _feed.CurrentFeedType == NewsFeed.FeedType.Favorites) _feed.Items.Remove(item);

                if (!_navigatingAway)
                {
                    AppBarUnfavorite.IsEnabled = true;
                    AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
        }

        private Button _btnAddSources;

        private void BtnAddSources_Loaded(object sender, RoutedEventArgs e)
        {
            _btnAddSources = sender as Button;
        }

        private void BtnAddSources_Click(object sender, RoutedEventArgs e)
        {
            PopupManageFeeds.IsOpen = true;
        }

        private void PopupManageFeeds_Closed(object sender, object e)
        {
            FeedManagementControl.ClearData();
        }

        private async void PopupManageFeeds_Opened(object sender, object e)
        {
            SbManageFeedsPopIn.Begin();
            if (!_feedManageVm.IsInitialised)
            {
                await _feedManageVm.InitFeeds();
            }
        }

        private void AppBar_Opened(object sender, object e)
        {
            if (ScrlVwrArticle.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                WebVwArticle.Margin = new Thickness(0, 0, 0, 88);
            }
        }

        private void AppBar_Closed(object sender, object e)
        {
            if (WebVwArticle.Margin.Bottom > 0)
            {
                WebVwArticle.Margin = new Thickness();
            }
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool handled = false;

            if (e.AddedItems.Count > 0)
            {
                NewsItem item = e.AddedItems[0] as NewsItem;
                if (item != null)
                {
                    LeftPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    AppBarFavorite.Visibility = item.IsFavorite ? Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
                    if (_showAppBarOnSelection) BottomAppBar.IsOpen = true;
                    handled = true;
                }
            }

            if (!handled)
            {
                LeftPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void FeedManageVm_FeedAdded(object sender, Feed addedFeed)
        {
            FeedItemViewModel vm = _nav.InsertFeed(addedFeed);
            if (vm != null) GrdVwNavigation.ScrollIntoView(vm);
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.DataContext is FeedItemViewModel)
                {
                    FeedItemViewModel vm = (FeedItemViewModel)button.DataContext;
                    MessageDialog dialog = new MessageDialog("Are you sure you wish to remove this feed?", "Confirmation");
                    dialog.Commands.Add(new UICommand("Remove", null, "Remove"));
                    dialog.Commands.Add(new UICommand("Cancel", null, null));
                    dialog.CancelCommandIndex = 1;
                    IUICommand result = await dialog.ShowAsync();
                    if (result.Id != null)
                    {
                        GrdVwNavigation.SelectedIndex = NavigationViewModel.DefaultInitialSelection;
                        _nav.RemoveFeed(vm);
                        _feedManageVm.RemoveFeed(vm);
                    }
                }
                else if (button.DataContext is CategoryViewModel)
                {
                    CategoryViewModel vm = (CategoryViewModel)button.DataContext;
                    MessageDialog dialog = new MessageDialog("This will remove all the feeds in this category. Are you sure?", "Confirmation");
                    dialog.Commands.Add(new UICommand("Remove", null, "Remove"));
                    dialog.Commands.Add(new UICommand("Cancel", null, null));
                    dialog.CancelCommandIndex = 1;
                    IUICommand result = await dialog.ShowAsync();
                    if (result.Id != null)
                    {
                        GrdVwNavigation.SelectedIndex = NavigationViewModel.DefaultInitialSelection;
                        List<FeedItemViewModel> feeds = _nav.GetCategoryFeeds(vm);
                        _nav.RemoveCategory(vm);
                        _feedManageVm.RemoveCategory(vm, feeds);
                    }
                }
            }
        }

        private void AppBarFontSize_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = DisplayUtilities.GetPopupElementRect(sender as FrameworkElement);
            PopupAppBarMenu.HorizontalOffset = rect.Left + 10;
            PopupAppBarMenu.VerticalOffset = -212;
            PopupAppBarMenu.IsOpen = true;
            //PopupMenu menu = new PopupMenu();
            //menu.Commands.Add(new UICommand("Small", null, WeaveOptions.FontSize.Small));
            //menu.Commands.Add(new UICommand("Medium", null, WeaveOptions.FontSize.Medium));
            //menu.Commands.Add(new UICommand("Large", null, WeaveOptions.FontSize.Large));
            //IUICommand result = await menu.ShowForSelectionAsync(DisplayUtilities.GetPopupElementRect(sender as FrameworkElement), Placement.Above);
            //if (result != null && result.Id is WeaveOptions.FontSize)
            //{
            //    WeaveOptions.CurrentFontSize = (WeaveOptions.FontSize)result.Id;
            //    if (ScrlVwrArticle.Visibility == Windows.UI.Xaml.Visibility.Visible)
            //    {
            //        String[] parameters = { ((int)WeaveOptions.CurrentFontSize).ToString() + "pt" };
            //        WebVwArticle.InvokeScript("setTextSize", parameters);
            //    }
            //}
        }

        private void PopupAppBarMenu_Opened(object sender, object e)
        {
            SbAppBarMenuPopIn.Begin();
        }

        private void PopupAppBarMenu_Closed(object sender, object e)
        {
        }

        private void FontSizeControl_FontSizeChanged(object sender, WeaveOptions.FontSize newSize)
        {
            if (ScrlVwrArticle.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                int fontSize = GetFontSize();
                String[] parameters = { fontSize.ToString() };
                WebVwArticle.InvokeScript("setTextSize", parameters);

                int articleWidth = GetArticleWidth(fontSize);
                AdjustArticleViewWidth(articleWidth + 160);
            }
            PopupAppBarMenu.IsOpen = false;
        }

        private void AdjustArticleViewWidth(int width)
        {
            if (_browserDisplayWidth != width)
            {
                _browserDisplayWidth = width;
                ScrlVwrArticle.Width = width;
                AnimArticleFlyIn.From = width;
                AnimArticleFlyOut.To = width;
            }
        }


    } // end of class
}
