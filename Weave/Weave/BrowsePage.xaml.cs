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
using Weave.Views.Browse;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
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
        public const String NavParamSelectedSpecialKey = "Special";

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

        private FontSizeSelection _fontSizeControl = new FontSizeSelection();
        private LayoutSizeSelection _layoutSizeControl = new LayoutSizeSelection();

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
            RectArticleSnapshot.Fill = _browserBrush;

            _fontSizeControl.FontSizeChanged += FontSizeControl_FontSizeChanged;
            _layoutSizeControl.LayoutSizeChanged += LayoutSizeControl_LayoutSizeChanged;
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

            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += ShareHandler;
            SettingsPane.GetForCurrentView().CommandsRequested += Page_CommandsRequested;

            if (navigationParameter != null && navigationParameter is Dictionary<String, object>)
            {
                Dictionary<String, object> parameters = (Dictionary<String, object>)navigationParameter;
                if (parameters.ContainsKey(NavParamSelectedCategoryKey)) _nav.InitialSelectedCategory = parameters[NavParamSelectedCategoryKey] as String;
                if (parameters.ContainsKey(NavParamSelectionKey)) _initialSelectedItemId = (Guid)parameters[NavParamSelectionKey];
                if (parameters.ContainsKey(NavParamSelectedSourceKey)) _nav.InitialSelectedFeed = (Guid)parameters[NavParamSelectedSourceKey];
                if (parameters.ContainsKey(NavParamSelectedSpecialKey)) _nav.InitialSelectedSpecial = (CategoryViewModel.CategoryType)parameters[NavParamSelectedSpecialKey];
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
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested -= ShareHandler;
            SettingsPane.GetForCurrentView().CommandsRequested -= Page_CommandsRequested;
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
                if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible) CloseArticle();
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
            if (e.NewSize.Height > 0)
            {
                double navTop = GrdVwNavigation.TransformToVisual(this).TransformPoint(new Point()).Y;
                double maxHeight = this.ActualHeight - (navTop + BtnAddSources.ActualHeight + BtnAddSources.Margin.Top + BtnAddSources.Margin.Bottom);
                GrdVwNavigation.MaxHeight = maxHeight;
            }

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

                UpdateTemplateSelector();
            }
        }

        private void UpdateTemplateSelector()
        {
            switch (WeaveOptions.CurrentLayoutSize)
            {
                case WeaveOptions.LayoutSize.Large:
                    itemGridView.ItemTemplateSelector = this.Resources["ArticleSelectorLarge"] as DataTemplateSelector;
                    break;
                default:
                    itemGridView.ItemTemplateSelector = this.Resources["ArticleSelector"] as DataTemplateSelector;
                    break;
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
            //if (feed.RequiresRefresh)
            //{
            //    refresh = true;
            //    feed.RequiresRefresh = false;
            //}
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
            else if (category.Type == CategoryViewModel.CategoryType.PreviousRead)
            {
                _feed.SetFeedParam(NewsFeed.FeedType.PreviousRead, null);
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
                if (item.IsNew) item.IsNew = false;
                int fontSize = GetFontSize();
                int articleWidth = GetArticleWidth(fontSize);
                AdjustArticleViewWidth(articleWidth + 160);
                RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ArticleContainer.DataContext = item;
                ArticleContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            ArticleContainer.DataContext = null;
            RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ArticleContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            WebVwArticle.NavigateToString("");
            GrdBrowserControls.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ArticleContainer.Width = _browserDisplayWidth;
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
                WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                if (_feed != null && _feed.CurrentFeedType != NewsFeed.FeedType.PreviousRead) _readTimer.Start();
            }
        }

        private void BrowseToWebPage(String url)
        {
            PrgRngBrowserLoading.IsActive = true;
            ArticleContainer.Width = 1000;
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

        private void WebVwArticle_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                Dictionary<String, String> parameters = ParseParameters(e.Value, '&');
                if (parameters.ContainsKey("LaunchLink"))
                {
                    //String id = parameters.ContainsKey("Id") ? parameters["Id"] : null;
                    //if (!String.Equals(id, "sg_link"))
                    //{
                    //    WebVwArticle.NavigateToString("");
                    //    await Task.Delay(50);
                    //    BrowseToWebPage(parameters["LaunchLink"]);
                    //}
                    //else
                    //{
                    //    Windows.System.Launcher.LaunchUriAsync(new Uri(parameters["LaunchLink"], UriKind.Absolute));
                    //}
                    Windows.System.Launcher.LaunchUriAsync(new Uri(parameters["LaunchLink"], UriKind.Absolute));
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
                UpdateAddSourceMargin();

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
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible) CloseArticle();
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
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible) CloseArticle();
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
                if (_feed != null && _feed.CurrentFeedType == NewsFeed.FeedType.Favorites)
                {
                    _feed.Items.Remove(item);
                    BottomAppBar.IsOpen = false;
                }

                if (!_navigatingAway)
                {
                    AppBarUnfavorite.IsEnabled = true;
                    AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
        }

        private void BtnAddSources_Click(object sender, RoutedEventArgs e)
        {
            PopupManageFeeds.IsOpen = true;
        }

        private void PopupManageFeeds_Closed(object sender, object e)
        {
            RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            FeedManagementControl.ClearData();
        }

        private async void PopupManageFeeds_Opened(object sender, object e)
        {
            RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SbManageFeedsPopIn.Begin();
            if (!_feedManageVm.IsInitialised)
            {
                await _feedManageVm.InitFeeds();
            }
        }

        private void AppBar_Opened(object sender, object e)
        {
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible)
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
            UpdateAddSourceMargin();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                PopupMenu menu = new PopupMenu();
                //if (button.DataContext is CategoryViewModel)
                //{
                //    menu.Commands.Add(new UICommand("Add source...", null, "Add"));
                //    menu.Commands.Add(new UICommandSeparator());
                //}
                menu.Commands.Add(new UICommand("Delete"));
                IUICommand result = await menu.ShowForSelectionAsync(DisplayUtilities.GetPopupElementRect(button), Placement.Below);
                if (result != null)
                {
                    if (button.DataContext is FeedItemViewModel)
                    {
                        FeedItemViewModel vm = (FeedItemViewModel)button.DataContext;
                        GrdVwNavigation.SelectedIndex = NavigationViewModel.DefaultInitialSelection;
                        _nav.RemoveFeed(vm);
                        _feedManageVm.RemoveFeed(vm);
                    }
                    else if (button.DataContext is CategoryViewModel)
                    {
                        CategoryViewModel vm = (CategoryViewModel)button.DataContext;
                        GrdVwNavigation.SelectedIndex = NavigationViewModel.DefaultInitialSelection;
                        List<FeedItemViewModel> feeds = _nav.GetCategoryFeeds(vm);
                        _nav.RemoveCategory(vm);
                        _feedManageVm.RemoveCategory(vm, feeds);
                        MainPage.RequireCategoryRefresh = true;
                    }

                    UpdateAddSourceMargin();
                }
            }
        }

        private void AppBarFontSize_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Rect rect = DisplayUtilities.GetPopupElementRect(button);

                SnapshotArticle(rect);
                GrdAppBarMenuContent.Children.Clear();
                GrdAppBarMenuContent.Children.Add(_fontSizeControl);
                PopupAppBarMenu.HorizontalOffset = rect.Left + 10;
                PopupAppBarMenu.VerticalOffset = -212;
                PopupAppBarMenu.IsOpen = true;

            }
        }

        /// <summary>
        /// Takes a snapshot of the article (if required) to ensure display of elements if overlapping (Windows 8).
        /// </summary>
        /// <param name="position"></param>
        private void SnapshotArticle(Rect position)
        {
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                Rect articleBounds = DisplayUtilities.GetPopupElementRect(ArticleContainer);
                if (position.Right > articleBounds.Left)
                {
                    _browserBrush.Redraw();
                    RectArticleSnapshot.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private void PopupAppBarMenu_Opened(object sender, object e)
        {
            SbAppBarMenuPopIn.Begin();
        }

        private void PopupAppBarMenu_Closed(object sender, object e)
        {
            if (RectArticleSnapshot.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                RectArticleSnapshot.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void FontSizeControl_FontSizeChanged(object sender, WeaveOptions.FontSize newSize)
        {
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                int fontSize = GetFontSize();
                String[] parameters = { fontSize.ToString() };
                WebVwArticle.InvokeScript("setTextSize", parameters);

                int articleWidth = GetArticleWidth(fontSize);
                AdjustArticleViewWidth(articleWidth + 160);
            }
            //UpdateTemplateSelector();
            PopupAppBarMenu.IsOpen = false;
        }

        private void AdjustArticleViewWidth(int width)
        {
            if (_browserDisplayWidth != width)
            {
                _browserDisplayWidth = width;
                ArticleContainer.Width = width;
                AnimArticleFlyIn.From = width;
                AnimArticleFlyOut.To = width;
            }
        }

        private void ShareHandler(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs e)
        {
            if (itemGridView.SelectedItem is NewsItem)
            {
                NewsItem item = (NewsItem)itemGridView.SelectedItem;
                Windows.ApplicationModel.DataTransfer.DataRequest request = e.Request;
                request.Data.Properties.Title = item.Title;
                request.Data.SetUri(new Uri(item.Link));
            }
            else e.Request.FailWithDisplayText("Select an article you'd like to share and try again.");
        }

        private void AppBarShare_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        private void UpdateAddSourceMargin()
        {
            return;
            double navBottom = GrdVwNavigation.TransformToVisual(this).TransformPoint(new Point()).Y + GrdVwNavigation.ActualHeight;
            double addSourcesTop = BtnAddSources.TransformToVisual(this).TransformPoint(new Point()).Y;
            double spaceBetween = addSourcesTop - navBottom - 20;

            if (spaceBetween > 0)
            {
                Thickness storeMargin = BtnAddSources.Margin;
                BtnAddSources.Tag = storeMargin;

                BtnAddSources.Margin = new Thickness(storeMargin.Left, -(spaceBetween + NavigationViewModel.NavSpacerHeight + 40), storeMargin.Right, storeMargin.Bottom);
            }
            else if (BtnAddSources.Tag is Thickness)
            {
                BtnAddSources.Margin = (Thickness)BtnAddSources.Tag;
                BtnAddSources.Tag = null;
            }
        }

        private void AppBarLayout_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Rect rect = DisplayUtilities.GetPopupElementRect(button);

                SnapshotArticle(rect);
                GrdAppBarMenuContent.Children.Clear();
                GrdAppBarMenuContent.Children.Add(_layoutSizeControl);
                PopupAppBarMenu.HorizontalOffset = rect.Left + 10;
                PopupAppBarMenu.VerticalOffset = -172;
                PopupAppBarMenu.IsOpen = true;

            }
        }

        private void LayoutSizeControl_LayoutSizeChanged(object sender, WeaveOptions.LayoutSize newSize)
        {
            PopupAppBarMenu.IsOpen = false;
            UpdateTemplateSelector();
        }

        private void PopupFlyout_Opened(object sender, object e)
        {
            SbFlyoutPopIn.Begin();
        }

        void Page_CommandsRequested(Windows.UI.ApplicationSettings.SettingsPane sender, Windows.UI.ApplicationSettings.SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("About", "About", new UICommandInvokedHandler(OnAboutClicked)));
        }

        private void OnAboutClicked(IUICommand command)
        {
            GrdFlyoutContent.Children.Clear();
            GrdFlyoutContent.Children.Add(new AboutFlyout());
            PopupFlyout.IsOpen = true;
        }


    } // end of class
}
