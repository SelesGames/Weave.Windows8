using Common.Microsoft.OneNote.Response;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weave.Common;
using Weave.Microsoft.OneNote;
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
        public const String NavParamAddSourceKey = "AddSource";

        private NewsFeed _feed = new NewsFeed();
        private NavigationViewModel _nav = new NavigationViewModel();
        private FeedManagementViewModel _feedManageVm = new FeedManagementViewModel();

        private int _initialFeedCount = 20;

        private const int DefaultBrowserWidth = 817;
        private int _browserDisplayWidth = DefaultBrowserWidth;

        private Stack<Uri> _browserBackStack = new Stack<Uri>();

        private Guid? _initialSelectedItemId = null;

        private DispatcherTimer _readTimer;
        private const double ReadInterval = 0.1;

        private bool _navigatingAway = false;

        private WebViewBrush _browserBrush = new WebViewBrush();

        private bool _showAppBarOnSelection = true;

        private FontSizeSelection _fontSizeControl = new FontSizeSelection();
        private LayoutSizeSelection _layoutSizeControl = new LayoutSizeSelection();
        private ReadingThemeSelection _readingThemeControl = new ReadingThemeSelection();
        ArticleViewSelection _articleSelectionControl = new ArticleViewSelection();

        private bool? _isMouse = null;

        private bool _ignoreScrollIntoView = false;

        private bool _initialAddFeed = false;

        private const int SnappedThreshold = App.BaseSnappedWidth + 100;

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
            _readingThemeControl.ReadingThemeChanged += ReadingThemeControl_ReadingThemeChanged;
            _articleSelectionControl.ArticleViewChanged += ArticleSelectionControl_ArticleViewChanged;

            EditFeedControl.SaveRequest += EditFeedControl_SaveRequest;
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
                if (parameters.ContainsKey(NavParamAddSourceKey)) _initialAddFeed = true;
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
                if (e.ClickedItem is AdvertisingNewsItem)
                {
                    AdvertisingNewsItem ad = (AdvertisingNewsItem)e.ClickedItem;
                    if (ad.ShowFallbackAd) ad.ExecuteFallbackAd();
                }
                else
                {
                    _showAppBarOnSelection = false;
                    itemGridView.SelectedItem = e.ClickedItem;
                    _showAppBarOnSelection = true;
                    ShowArticle(e.ClickedItem as NewsItem);
                }
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

        private VariableSizedWrapGrid _itemsContentPanel = null;

        private void WrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_itemsContentPanel == null && sender is VariableSizedWrapGrid)
            {
                _itemsContentPanel = (VariableSizedWrapGrid)sender;
                _itemsContentPanel.Tag = _itemsContentPanel.Margin;

                SetViewMode();

                GridView_Loaded(itemGridView, null);
            }
        }

        private bool _pageLoaded = false;

        private async void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = true;
            int initialSelection = await _nav.Initialise();
            GrdVwNavigation.DataContext = _nav;
            GrdVwNavigation.SelectedIndex = initialSelection;
            UpdateArticleContainerBackground();
            AppBarPositionCenter.Visibility = WeaveOptions.CurrentArticlePlacement == WeaveOptions.ArticlePlacement.Center ? Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0)
            {
                double navTop = GrdVwNavigation.TransformToVisual(this).TransformPoint(new Point()).Y;
                double maxHeight = this.ActualHeight - (navTop + BtnAddSources.ActualHeight + BtnAddSources.Margin.Top + BtnAddSources.Margin.Bottom);
                GrdVwNavigation.MaxHeight = maxHeight;
            }
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible && e.PreviousSize.Width > 0 && e.PreviousSize.Width < SnappedThreshold && e.NewSize.Width > SnappedThreshold)
            {
                // close article if going from snapped to expanded state
                CloseArticle();
            }
            SetViewMode();
        }

        private void SetViewMode()
        {
            if (_pageLoaded)
            {
                Size size = new Size(this.ActualWidth, this.ActualHeight);
                if (size.Width > 0 && size.Height > 0)
                {
                    if (size.Width < SnappedThreshold)
                    {
                        if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible) CloseArticle();
                        Grid.SetRow(itemGridView, 1);
                        StkPnlNavigation.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        backButton.Style = App.Current.Resources["SnappedBackButtonStyle"] as Style;
                        BtnMenuSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        RectNavBackgroundSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        _itemsContentPanel.Margin = new Thickness(0,20,0,40);
                    }
                    else
                    {
                        if (size.Height > size.Width)
                        {
                            Grid.SetRow(itemGridView, 0);
                            StkPnlNavigation.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            backButton.Style = App.Current.Resources["PortraitBackButtonStyle"] as Style;
                            BtnMenuSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            RectNavBackgroundSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            if (_itemsContentPanel.Tag != null) _itemsContentPanel.Margin = (Thickness)_itemsContentPanel.Tag;
                        }
                        else
                        {
                            Grid.SetRow(itemGridView, 0);
                            StkPnlNavigation.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            backButton.Style = App.Current.Resources["BackButtonStyle"] as Style;
                            BtnMenuSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            RectNavBackgroundSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            if (_itemsContentPanel.Tag != null) _itemsContentPanel.Margin = (Thickness)_itemsContentPanel.Tag;
                        }
                    }
                }
                UpdateTemplateSelector();
            }
        }

        private void UpdateTemplateSelector()
        {
            Size size = new Size(this.ActualWidth, this.ActualHeight);
            if (size.Width < SnappedThreshold)
            {
                itemGridView.ItemTemplateSelector = this.Resources["ArticleSelectorSnapped"] as DataTemplateSelector;
            }
            else
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
        }

        private async void GrdVwNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageLoaded && e.AddedItems.Count > 0)
            {
                await ProcessSelectedNav(GrdVwNavigation.SelectedItem);

                if (_initialSelectedItemId != null)
                {
                    NewsItem initialSelection = _feed.FindItemById(_initialSelectedItemId.Value);
                    if (initialSelection != null)
                    {
                        _showAppBarOnSelection = false;
                        itemGridView.SelectedItem = initialSelection;
                        _showAppBarOnSelection = true;
                        await Task.Delay(1); // allow rendering of page
                        ShowArticle(initialSelection, true, false);
                        _initialSelectedItemId = null;
                    }
                }
                if (_initialAddFeed)
                {
                    itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    PopupManageFeeds.IsOpen = true;
                }
            }
        }

        private async Task ProcessSelectedNav(object selected, bool refresh = false)
        {
            if (MainScrollViewer != null) MainScrollViewer.ScrollToVerticalOffset(0);
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _feed.ClearData();
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
                _feed.SetFeedParam(NewsFeed.FeedType.Category, category.Info.Category);
                if (category.Type == CategoryViewModel.CategoryType.All)
                {
                    _nav.ClearAllNewCounts();
                }
                else
                {
                    _nav.ClearCategoryNewCount(category);
                    if (category.RequiresRefresh)
                    {
                        category.RequiresRefresh = false;
                        entry = EntryType.ExtendRefresh;
                    }
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

        public int GetArticleWidth(int fontSize)
        {
            int width = (int)(fontSize * 43);
            if (this.ActualWidth < SnappedThreshold) width = (int)this.ActualWidth - 40;
            else if (width > this.ActualWidth - MinimumArticleTextPadding) width = (int)this.ActualWidth - MinimumArticleTextPadding;
            return width;
        }

        private const int MinimumArticleTextPadding = 160;

        private void ShowArticle(NewsItem item, bool allowMobilizer = true, bool showLoading = true)
        {
            _browserBackStack.Clear();
            BtnBrowserBack.IsEnabled = false;
            if (item != null)
            {
                if (WeaveOptions.CurrentArticlePlacement == WeaveOptions.ArticlePlacement.Center) ArticleContainer.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                if (item.IsNew) item.IsNew = false;
                int fontSize = GetFontSize();
                int articleWidth = GetArticleWidth(fontSize);
                AdjustArticleViewWidth(articleWidth);
                RectOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ArticleContainer.DataContext = item;
                ArticleContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                PrgRngArticleLoading.IsActive = true;
                if (showLoading) WebVwArticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                SbArticleFlyIn.Begin();
                ParseArticle(item, fontSize, articleWidth, allowMobilizer);
                RightPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                AppBarArticleView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void CloseArticle()
        {
            _readTimer.Stop();
            Point p = ArticleContainer.TransformToVisual(this).TransformPoint(new Point());
            AnimArticleFlyOut.To = Window.Current.Bounds.Width - p.X;
            SbArticleFlyOut.Begin();
            RightPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            AppBarFontSize.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AppBarReadingTheme.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AppBarArticleView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BtnBackArticlePortrait.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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

        private async void ParseArticle(NewsItem item, int fontSize, int articleWidth, bool allowMobilizer)
        {
            if (item != null)
            {
                PrgRngBrowserLoading.IsActive = true;
                bool loadWebBrowser = true;

                if (allowMobilizer && item.Feed.ArticleViewingType == ArticleViewingType.Mobilizer)
                {
                    loadWebBrowser = false;
                    String result = await MobilizerHelper.GetMobilizedHtml(item, fontSize, articleWidth);
                    if (result != null)
                    {
                        AppBarFontSize.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        AppBarReadingTheme.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        WebVwArticle.NavigateToString(result);
                    }
                    else
                    {
                        loadWebBrowser = true;
                    }
                }

                if (loadWebBrowser)
                {
                    AppBarFontSize.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    AppBarReadingTheme.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                if (_ignoreScrollIntoView) _ignoreScrollIntoView = false;
                else GrdVwNavigation.ScrollIntoView(GrdVwNavigation.Items[GrdVwNavigation.SelectedIndex]);
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
            Refresh();
        }

        private async void Refresh()
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

                bool success = await UserHelper.Instance.AddFavorite(item);

                if (!_navigatingAway)
                {
                    AppBarFavorite.IsEnabled = true;
                    if (success)
                    {
                        AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        App.ShowStandardError("Something went wrong adding this article to your favorites.");
                    }
                }
            }
        }

        private async void AppBarUnfavorite_Click(object sender, RoutedEventArgs e)
        {
            NewsItem item = itemGridView.SelectedItem as NewsItem;
            if (item != null)
            {
                AppBarUnfavorite.IsEnabled = false;

                bool success = await UserHelper.Instance.RemoveFavorite(item);

                if (!_navigatingAway)
                {
                    AppBarUnfavorite.IsEnabled = true;
                    if (success)
                    {
                        if (_feed != null && _feed.CurrentFeedType == NewsFeed.FeedType.Favorites)
                        {
                            _feed.Items.Remove(item);
                            BottomAppBar.IsOpen = false;
                        }

                        AppBarFavorite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        App.ShowStandardError("Something went wrong removing this article to your favorites.");
                    }
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
                    if (item is AdvertisingNewsItem) itemGridView.SelectedItem = null;
                    else
                    {
                        LeftPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        AppBarFavorite.Visibility = item.IsFavorite ? Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
                        if (_showAppBarOnSelection) BottomAppBar.IsOpen = true;
                        handled = true;
                    }
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
            if (vm != null && !_ignoreScrollIntoView) GrdVwNavigation.ScrollIntoView(vm);
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
                if (button.DataContext is FeedItemViewModel)
                {
                    menu.Commands.Add(new UICommand("Edit...", null, "Edit"));
                }
                menu.Commands.Add(new UICommand("Delete", null, "Delete"));
                IUICommand result = await menu.ShowForSelectionAsync(DisplayUtilities.GetPopupElementRect(button), Placement.Below);
                if (result != null)
                {
                    if (String.Equals(result.Id, "Delete"))
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
                    }
                    else if (String.Equals(result.Id, "Edit"))
                    {
                        EditFeedControl.LoadFeed(button.DataContext as FeedItemViewModel);
                        Rect rect = DisplayUtilities.GetPopupElementRect(button);
                        PopupEditFeed.HorizontalOffset = rect.Left - (EditFeedControl.Width / 2);
                        PopupEditFeed.VerticalOffset = rect.Top + button.ActualHeight;
                        PopupEditFeed.IsOpen = true;
                    }
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
                AdjustArticleViewWidth(articleWidth);
            }
            //UpdateTemplateSelector();
            PopupAppBarMenu.IsOpen = false;
        }

        private void AdjustArticleViewWidth(int width)
        {
            switch (WeaveOptions.CurrentFontSize)
            {
                case WeaveOptions.FontSize.Small:
                    width += 160;
                    break;
                case WeaveOptions.FontSize.Medium:
                    width += 200;
                    break;
                case WeaveOptions.FontSize.Large:
                    width += 220;
                    break;
                default:
                    break;
            }

            if (this.ActualHeight > this.ActualWidth)
            {
                width = (int)this.ActualWidth;
                BtnBackArticlePortrait.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

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
                request.Data.Properties.Description = item.Title;
                request.Data.SetUri(new Uri(item.Link));
            }
            else e.Request.FailWithDisplayText("Select an article you'd like to share and try again.");
        }

        private void AppBarShare_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        private delegate Task<BaseResponse> AsyncAction(string token);

        private async void AppBarSaveToOneNote_Click(object sender, RoutedEventArgs e)
        {
            LiveAccountHelper helper = LiveAccountHelper.Instance;
            if (!LiveAccountHelper.Instance.LoginChecked) await LiveAccountHelper.Instance.SilentSignIn();

            if (helper.IsSignedIn)
            {
                String token = helper.AuthClient.Session.AccessToken;
                NewsItem selectedNewsItem = itemGridView.SelectedItem as NewsItem;
                if (selectedNewsItem != null)
                {
                    ArticleViewingType viewType = selectedNewsItem.Feed.ArticleViewingType;
                    bool isMobilized = (viewType == ArticleViewingType.Mobilizer || viewType == ArticleViewingType.MobilizerOnly);
                    if (IsArticleOpen && GrdBrowserControls.Visibility == Windows.UI.Xaml.Visibility.Visible) isMobilized = false;

                    AsyncAction saveTask;
                    AppBarSaveToOneNote.IsEnabled = false;
                    PrgRngSharingOneNote.IsActive = true;
                    
                    if (isMobilized)
                    {
                        String imageUrl = null;
                        if (selectedNewsItem.HasImage)
                        {
                            if (selectedNewsItem.Image != null) imageUrl = selectedNewsItem.Image.OriginalUrl;
                            else if (!String.IsNullOrEmpty(selectedNewsItem.ImageUrl)) imageUrl = selectedNewsItem.ImageUrl;
                        }

                        String bodyHtml = await MobilizerHelper.GetMobilizedBody(selectedNewsItem);

                        var oneNoteSave = new MobilizedOneNoteItem
                        {
                            Title = selectedNewsItem.Title,
                            Link = selectedNewsItem.Link,
                            Source = selectedNewsItem.FormattedForMainPageSourceAndDate,
                            HeroImage = imageUrl,
                            BodyHtml = bodyHtml,
                        };
                        saveTask = oneNoteSave.SendToOneNote;
                    }
                    else
                    {
                        var oneNoteSave = new HtmlLinkOneNoteItem
                        {
                            Title = selectedNewsItem.Title,
                            Link = selectedNewsItem.Link,
                            Source = selectedNewsItem.FormattedForMainPageSourceAndDate,
                        };
                        saveTask = oneNoteSave.SendToOneNote;
                    }

                    try
                    {
                        BaseResponse response = await saveTask(token);
                        //MessageDialog dialog = new MessageDialog(String.Format("response code: {0}", response.StatusCode));
                        //dialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                    }

                    PrgRngSharingOneNote.IsActive = false;
                    AppBarSaveToOneNote.IsEnabled = true;
                }
            }
            else
            {
                OneNoteFlyout flyout = new OneNoteFlyout();
                flyout.ShowIndependent();
            }
            // For an example of how to save to OneNote, see Weave WP8 project
            // class:  ReadabilityPage.xaml.cs
            // line: 682 (SendToOneNoteMenuItemClick function)
            // TODO: open some sort of flyout/page or something, and show the LiveSDK login button
            /*
             * 
            psuedo-code:
            
            var articleViewType = viewModel.NewsItem.Feed.ArticleViewingType;
            Func<Task<BaseResponse>> saveTask;
            
            if ((articleViewType == ArticleViewingType.Mobilizer || articleViewType == ArticleViewingType.MobilizerOnly)
                && viewModel.CurrentMobilizedArticle != null)
            {
                var mobilizedArticle = viewModel.CurrentMobilizedArticle;

                var oneNoteSave = new MobilizedOneNoteItem
                {
                    Title = mobilizedArticle.Title,
                    Link = mobilizedArticle.Link,
                    Source = mobilizedArticle.CombinedPublicationAndDate,
                    HeroImage = mobilizedArticle.HeroImageUrl,
                    BodyHtml = mobilizedArticle.ContentHtml,
                };
                saveTask = () => oneNoteSave.SendToOneNote(token);
            }

            else
            {
                var oneNoteSave = new HtmlLinkOneNoteItem
                {
                    Title = viewModel.NewsItem.Title,
                    Link = viewModel.NewsItem.Link,
                    Source = viewModel.NewsItem.FormattedForMainPageSourceAndDate,
                };
                saveTask = () => oneNoteSave.SendToOneNote(token);
            }
            
            frame.OverlayText = "Saving to OneNote...";
            frame.IsLoading = true;
            var response = await saveTask();
            frame.IsLoading = false;
            */
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
            args.Request.ApplicationCommands.Add(new SettingsCommand("Live Account", "Live Account", new UICommandInvokedHandler(OnLiveAccountClicked)));
        }

        private void OnAboutClicked(IUICommand command)
        {
            GrdFlyoutContent.Children.Clear();
            GrdFlyoutContent.Children.Add(new AboutFlyout());
            PopupFlyout.IsOpen = true;
        }

        private void OnLiveAccountClicked(IUICommand command)
        {
            OneNoteFlyout flyout = new OneNoteFlyout();
            flyout.Show();
        }

        private void AppBarReadingTheme_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Rect rect = DisplayUtilities.GetPopupElementRect(button);

                SnapshotArticle(rect);
                GrdAppBarMenuContent.Children.Clear();
                GrdAppBarMenuContent.Children.Add(_readingThemeControl);
                PopupAppBarMenu.HorizontalOffset = rect.Left + 22;
                PopupAppBarMenu.VerticalOffset = -172;
                PopupAppBarMenu.IsOpen = true;

            }
        }

        private void ReadingThemeControl_ReadingThemeChanged(object arg1, WeaveOptions.ReadingTheme arg2)
        {
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                int fontSize = GetFontSize();
                String[] parameters = { MobilizerHelper.GetForeground(), MobilizerHelper.GetBackground() };
                WebVwArticle.InvokeScript("colorFontAndBackground", parameters);
                UpdateArticleContainerBackground();
            }
            PopupAppBarMenu.IsOpen = false;
        }

        private void UpdateArticleContainerBackground()
        {
            if (WeaveOptions.CurrentReadingTheme == WeaveOptions.ReadingTheme.Light) ArticleContainer.Background = MobilizerHelper.LightBackgroundBrush;
            else ArticleContainer.Background = MobilizerHelper.DarkBackgroundBrush;
        }

        private void RectOverlay_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            bool close = true;
            Pointer p = e.Pointer;
            if (p.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                Windows.UI.Input.PointerPoint ptrPt = e.GetCurrentPoint(RectOverlay);
                if (ptrPt.Properties.IsRightButtonPressed) close = false;
            }

            if (close && ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible) CloseArticle();
        }


        private void PopupEditFeed_Opened(object sender, object e)
        {
            SbEditFeedPopIn.Begin();
        }

        void EditFeedControl_SaveRequest(object sender, FeedItemViewModel feed)
        {
            // rebind model so it updates the view in the nav
            Feed model = feed.Feed;
            feed.Feed = null;
            feed.Feed = model;

            PopupEditFeed.IsOpen = false;
            Refresh();
        }

        private async void AppBarMarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            object selected = GrdVwNavigation.SelectedItem;
            AppBarMarkAllRead.IsEnabled = false;
            if (selected is CategoryViewModel)
            {
                CategoryViewModel vm = (CategoryViewModel)selected;
                if (vm.Type == CategoryViewModel.CategoryType.Specific)
                {
                    await UserHelper.Instance.MarkCategoryAsRead(vm.Info.Category);
                    MarkAllArticlesRead();
                }
                else if (vm.Type == CategoryViewModel.CategoryType.Latest)
                {
                    await UserHelper.Instance.MarkSoftRead(UserHelper.Instance.GetLatestNews());
                    MarkAllArticlesRead();
                }
            }
            else if (selected is FeedItemViewModel)
            {
                FeedItemViewModel vm = (FeedItemViewModel)selected;
                await UserHelper.Instance.MarkFeedAsRead(vm.Feed);
                MarkAllArticlesRead();
            }
            AppBarMarkAllRead.IsEnabled = true;
        }

        private void MarkAllArticlesRead()
        {
            if (_feed != null)
            {
                foreach (NewsItem item in _feed.Items)
                {
                    if (!item.HasBeenViewed) item.HasBeenViewed = true;
                }
            }
        }

        private void BtnExpanded_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is CategoryViewModel)
            {
                CategoryViewModel category = (CategoryViewModel)button.DataContext;
                _ignoreScrollIntoView = true;
                _nav.CollapseCategory(category);
            }
        }

        private void BtnCollapsed_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is CategoryViewModel)
            {
                CategoryViewModel category = (CategoryViewModel)button.DataContext;
                _ignoreScrollIntoView = true;
                _nav.ExpandCategory(category);
            }
        }

        private void ArticleSelectionControl_ArticleViewChanged(object sender, bool useMobilizer)
        {
            if (ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible && ArticleContainer.DataContext is NewsItem)
            {
                NewsItem item = (NewsItem)ArticleContainer.DataContext;
                WebVwArticle.NavigateToString("");
                ShowArticle(item, useMobilizer);
            }
            PopupAppBarMenu.IsOpen = false;
        }

        private void AppBarArticleView_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Rect rect = DisplayUtilities.GetPopupElementRect(button);

                SnapshotArticle(rect);
                GrdAppBarMenuContent.Children.Clear();
                GrdAppBarMenuContent.Children.Add(_articleSelectionControl);
                _articleSelectionControl.SetArticleView(GrdBrowserControls.Visibility == Windows.UI.Xaml.Visibility.Collapsed);
                PopupAppBarMenu.HorizontalOffset = rect.Left + 15;
                PopupAppBarMenu.VerticalOffset = -172;
                PopupAppBarMenu.IsOpen = true;
            }
        }

        private void AdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            var adControl = sender as AdControl;
            if (adControl != null)
            {
                AdvertisingNewsItem item = adControl.DataContext as AdvertisingNewsItem;
                if (item != null)
                {
                    item.SelectFallbackAd(false);
                }
            }
        }

        private void AdControlLarge_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            var adControl = sender as AdControl;
            if (adControl != null)
            {
                AdvertisingNewsItem item = adControl.DataContext as AdvertisingNewsItem;
                if (item != null)
                {
                    item.SelectFallbackAd(true);
                }
            }
        }

        private void AppBarPositionCenter_Click(object sender, RoutedEventArgs e)
        {
            WeaveOptions.CurrentArticlePlacement = WeaveOptions.ArticlePlacement.Center;
            ArticleContainer.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            AppBarPositionCenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void AppBarPositionRight_Click(object sender, RoutedEventArgs e)
        {
            WeaveOptions.CurrentArticlePlacement = WeaveOptions.ArticlePlacement.Right;
            ArticleContainer.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
            AppBarPositionCenter.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void SbManageFeedsPopIn_Completed(object sender, object e)
        {
            if (_initialAddFeed)
            {
                _initialAddFeed = false;
                await Task.Delay(1500); // delay to fix weird rendering order bug that overlays content over popup on initial load
                itemGridView.Visibility = Visibility.Visible;
            }
        }

        private async void GrdVwNavigationSnapped_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageLoaded && e.AddedItems.Count > 0)
            {
                PopupNavSnapped.IsOpen = false;
                await ProcessSelectedNav(GrdVwNavigationSnapped.SelectedItem);
            }
        }

        private void BtnMenuSnapped_Click(object sender, RoutedEventArgs e)
        {
            PopupNavSnapped.IsOpen = true;
        }

        private void PopupNavSnapped_Opened(object sender, object e)
        {
            SbNavSnappedPopIn.Begin();
        }

        private bool IsArticleOpen
        {
            get { return ArticleContainer.Visibility == Windows.UI.Xaml.Visibility.Visible; }
        }

    } // end of class
}
