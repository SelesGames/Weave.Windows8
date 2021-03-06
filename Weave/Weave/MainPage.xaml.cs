﻿using System;
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
using Weave.ViewModels.Browse;
using Windows.Storage;
using Windows.UI.Popups;
using Weave.Views.StartHub;
using Windows.UI.ApplicationSettings;
using Microsoft.Advertising.WinRT.UI;

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
        private static StartLoginViewModel _loginVm = new StartLoginViewModel();
        private static StartAdvertisingViewModel _advertisingVm = new StartAdvertisingViewModel();

        private const double BaseSectionHeight = 450; // base height of each major section (profile, timeline, photos, etc)
        private static double _savedScrollPosition = 0;

        private static int _defaultStartItemCount; // default item count excluding clusters
        private const int AppendixItemCount = 2;

        private static ObservableCollection<StartItemBase> _startItems = new ObservableCollection<StartItemBase>();

        private const String CustomHeroIdKey = "CustomHeroId"; // stores the custom hero article id for showcasing

        private const String ItemOrder = "ItemOrder"; // stores the order of the items
        private const String LatestArticlesIndexKey = "LatestArticlesIndex";
        private const String SourcesIndexKey = "SourcesIndex";

        private const int DynamicThreshold = 700;

        public MainPage()
        {
            this.InitializeComponent();

            if (_startItems.Count == 0)
            {
                _startItems.Add(_heroArticleVm);
                _startItems.Add(_latestArticlesVm);
                _startItems.Add(_sourcesVm);
                _startItems.Add(_advertisingVm);
                _startItems.Add(_addVm);
                _startItems.Add(_loginVm);

                _defaultStartItemCount = _startItems.Count - AppendixItemCount;
            }

            _startItems.CollectionChanged += _startItems_CollectionChanged;

#if DEBUG
            AppBarClearRoaming.Visibility = Windows.UI.Xaml.Visibility.Visible;
#endif
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            ClusterHelper.ClusterRemoved += ClusterHelper_ClusterRemoved;
            Weave.Views.StartHub.LatestArticles.HeroSelected += LatestArticles_HeroSelected;
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += ShareHandler;
            UserHelper.Instance.UserChanged += UserChanged;
            SettingsPane.GetForCurrentView().CommandsRequested += Page_CommandsRequested;
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            ClusterHelper.ClusterRemoved -= ClusterHelper_ClusterRemoved;
            Weave.Views.StartHub.LatestArticles.HeroSelected -= LatestArticles_HeroSelected;
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested -= ShareHandler;
            UserHelper.Instance.UserChanged -= UserChanged;
            SettingsPane.GetForCurrentView().CommandsRequested -= Page_CommandsRequested;

            ApplicationViewState viewState = ApplicationView.Value;
            if (viewState == ApplicationViewState.FullScreenPortrait || viewState == ApplicationViewState.Snapped)
            {
            }
            else
            {
                _savedScrollPosition = MainScrollViewer.HorizontalOffset;
            }
        }

        private void PopupFlyout_Opened(object sender, object e)
        {
            SbFlyoutPopIn.Begin();
        }

        private void semanticZoomControl_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
            {
                GrdMainLogo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                GrdMainLogo.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (_semanticItemTapped)
                {
                    _semanticItemTapped = false;
                    StartItemBase sourceItem = e.SourceItem.Item as StartItemBase;
                    if (sourceItem != null)
                    {
                        bool isPortrait = ApplicationView.Value == ApplicationViewState.FullScreenPortrait;
                        int index = _startItems.IndexOf(sourceItem);
                        if (index < 1)
                        {
                            if (isPortrait) MainScrollViewer.ScrollToVerticalOffset(0);
                            else MainScrollViewer.ScrollToHorizontalOffset(0);
                        }
                        else if (index >= _startItems.Count - 2)
                        {
                            if (isPortrait) MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.ScrollableHeight);
                            else MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.ScrollableWidth);
                        }
                        else LstVwMain.ScrollIntoView(sourceItem, ScrollIntoViewAlignment.Leading);
                    }
                }
            }
        }

        private bool _semanticItemTapped = false;
        private void ZoomedOutItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StartItemBase sourceItem = ((FrameworkElement)sender).DataContext as StartItemBase;
            if (sourceItem != null)
            {
                _semanticItemTapped = true;
            }
        }

        private bool _mainScrollNotificationBound = false;

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
            PrgRngLoadingMain.IsActive = true;
            if (UserHelper.Instance.IsNewUser)
            {
                FirstLaunchView control = new FirstLaunchView();
                control.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                control.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                control.Completed += FirstLaunchControl_Completed;
                GrdFirstLaunch.Children.Clear();
                GrdFirstLaunch.Children.Add(control);

                GrdMainLogo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                GrdFirstLaunch.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                await InitMainPage();
            }
        }

        private async Task InitMainPage()
        {
            GrdMainLogo.Visibility = Windows.UI.Xaml.Visibility.Visible;
            PrgRngLoadingMain.IsActive = true;

            if (!_mainScrollNotificationBound)
            {
                App.RegisterForNotification("HorizontalOffset", MainScrollViewer, 0, MainScrollChanged);
                _mainScrollNotificationBound = true;
            }

            if (_latestArticlesVm.Items.Count == 0)
            {
                bool success = false;
                try
                {
                    success = await UserHelper.Instance.LoadUser();
                }
                catch (Exception e)
                {
                    App.LogError("Error initialising user", e);
                    success = false;
                }
                if (success) await LoadViewModels();
                else
                {
                    MessageDialog dialog = new MessageDialog("There was a problem connecting to Weave servers. Please restart the app and try again", "Oops!");
                    dialog.Commands.Add(new UICommand("Ok", null, null));
                    dialog.ShowAsync();
                    _startItems.Clear();
                }
            }
            else // check if anything requires refreshing
            {
                if (RequireAllRefresh || UserHelper.Instance.RequireUserRefresh)
                {
                    RequireAllRefresh = false;
                    await Refresh();
                }
                else
                {
                    if (RequireCategoryRefresh)
                    {
                        RequireCategoryRefresh = false;
                        _sourcesVm.InitSources();
                    }
                }
            }
            LstVwMain.ItemsSource = _startItems;
            PrgRngLoadingMain.IsActive = false;
            BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (_savedScrollPosition > 0)
            {
                ImgMainLogo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                await Task.Delay(1); // allow scroll to render
                MainScrollViewer.ScrollToHorizontalOffset(_savedScrollPosition);
                UpdateMainLogo(_savedScrollPosition);
                _savedScrollPosition = 0;
                ImgMainLogo.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private int FindHeroArticleIndex(List<StartNewsItemContainer> items)
        {
            int heroIndex = 0;
            int index = 0;
            int largestSize = 0;
            int currentSize;
            NewsItem news;

            Guid? customId = LoadCustomHeroId();

            foreach (StartNewsItemContainer container in items)
            {
                news = container.NewsItem;
                if (customId != null && news.Id == customId)
                {
                    heroIndex = index;
                    break;
                }

                if (news.Image != null)
                {
                    currentSize = news.Image.Width * news.Image.Height;
                    if (currentSize > largestSize)
                    {
                        heroIndex = index;
                        largestSize = currentSize;
                    }
                }
                index++;
            }

            return heroIndex;
        }

        private async Task LoadViewModels()
        {
            LstBxCategorySelector.ItemsSource = null;
            _loginVm.InitLoginItems();
            List<NewsItem> newsItems = UserHelper.Instance.GetLatestNews();
            UpdateLiveTileArticles(newsItems);
            if (newsItems != null && newsItems.Count > 0)
            {
                List<StartNewsItemContainer> viewItems = new List<StartNewsItemContainer>();
                int maxCount = StartLatestViewModel.BaseDisplayCount + (StartLatestViewModel.ExtraRows * 3) + 1; // add one for extraction of hero article
                for (int i = 0; i < newsItems.Count && i < maxCount; i++)
                {
                    viewItems.Add(new StartNewsItemContainer(new NewsItemIcon(newsItems[i])));
                }
                int heroIndex = FindHeroArticleIndex(viewItems);
                _heroArticleVm.Article = viewItems[heroIndex].NewsItem;
                viewItems.RemoveAt(heroIndex);
                _latestArticlesVm.InitItems(viewItems);
            }

            _sourcesVm.InitSources();

            if (_startItems.Count > _defaultStartItemCount + AppendixItemCount) ClearClusters();
            await InitClusters();
            RepositionSections();
            RefreshClusters();
        }

        public const double BaseHeight = 768; // base height of design screen
        private const double BinHeight = 280; // extra height to incorporte next bin of results

        private double _expansionFactor;

        private void AdjustForScreenResolution()
        {
            if (this.ActualHeight > this.ActualWidth)
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
                StartSourcesViewModel.ExtraRows = (int)(_expansionFactor * 5);
            }
            else
            {
                GrdRoot.Tag = BaseSectionHeight;
                StartClusterViewModel.ExtraRows = 0;
                StartLatestViewModel.ExtraRows = 0;
                StartSourcesViewModel.ExtraRows = 0;
            }
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustForScreenResolution();
            if (LstVwMain.ItemsSource != null) BottomAppBar.Visibility = ApplicationView.Value == ApplicationViewState.Snapped ? Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            SetViewMode(e.NewSize);
        }

        private void MainScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateMainLogo((double)MainScrollViewer.HorizontalOffset);
        }

        private const int LogoMinX = -100;
        private const int LogoMinY = -50;
        private const int LogoStartOffset = 1000;
        private const double LogoMovementScaleX = 10;
        private const double LogoRelativeScaleY = 2;

        private void UpdateMainLogo(double val)
        {
            if (val > LogoStartOffset)
            {
                double newX = (LogoStartOffset - val) / LogoMovementScaleX;
                double newY = newX / LogoRelativeScaleY;

                if (newX < LogoMinX) newX = LogoMinX;
                if (newY < LogoMinY) newY = LogoMinY;

                TranslateLogo.X = newX;
                TranslateLogo.Y = newY;
            }
            else
            {
                TranslateLogo.X = 0;
                TranslateLogo.Y = 0;
            }
        }

        private Button _addClusterButton;

        private void AddView_Loaded(object sender, RoutedEventArgs e)
        {
            Weave.Views.StartHub.AddView view = sender as Weave.Views.StartHub.AddView;
            if (view != null)
            {
                _addClusterButton = view.AddButton;
                _addClusterButton.Click -= BtnAddCluster_Click;
                _addClusterButton.Click += BtnAddCluster_Click;
            }
        }

        private void GrdPopupListContent_Loaded(object sender, RoutedEventArgs e)
        {
            GrdPopupListContent = sender as Grid;
            ResizeAddPopup();
        }

        private void ResizeAddPopup()
        {
            if (GrdPopupListContent != null)
            {
                ApplicationViewState viewState = ApplicationView.Value;
                double baseHeight = BinHeight + (BinHeight / 2) - 10;
                if (_expansionFactor > 0 && viewState != ApplicationViewState.Snapped)
                {
                    GrdPopupListContent.Height = baseHeight + (BinHeight * (int)_expansionFactor);
                }
                else
                {
                    GrdPopupListContent.Height = baseHeight;
                }

                Point p = new Point();

                if (viewState == ApplicationViewState.Snapped) GrdPopupListContent.Width = 280;
                else GrdPopupListContent.Width = 350;

                bool displayAbove = viewState == ApplicationViewState.FullScreenPortrait || viewState == ApplicationViewState.Snapped;

                if (_addClusterButton != null)
                {
                    p = _addClusterButton.TransformToVisual(this).TransformPoint(p);
                    if (displayAbove)
                    {
                        double remainingHeight = Window.Current.Bounds.Bottom - (p.Y + _addClusterButton.ActualHeight);
                        if (remainingHeight < 0)
                        {
                            p.X += remainingHeight;
                        }
                    }
                    else
                    {
                        double remainingWidth = Window.Current.Bounds.Right - p.X;
                        if (remainingWidth < GrdPopupListContent.Width)
                        {
                            p.X -= (GrdPopupListContent.Width - remainingWidth);
                        }
                    }
                }

                if (displayAbove)
                {
                    GrdPopupListContent.Margin = new Thickness(p.X, p.Y - GrdPopupListContent.Height + 60, 0, 0);
                }
                else
                {
                    GrdPopupListContent.Margin = new Thickness(p.X, p.Y, 0, 0);
                }
            }
        }

        private List<Object> InitCategoryListings()
        {
            List<Object> categoryListings = new List<object>();

            String noCategoryKey = "";
            Dictionary<String, List<Feed>> categoryFeeds = UserHelper.Instance.CategoryFeeds;
            if (categoryFeeds != null && categoryFeeds.Count > 0)
            {
                List<String> orderedKeys = new List<string>(categoryFeeds.Keys.OrderBy(s => s));
                foreach (String category in orderedKeys)
                {
                    if (!String.Equals(category, noCategoryKey))
                    {
                        categoryListings.Add(new CategoryViewModel() { DisplayName = category, Info = new CategoryInfo() { Category = category } });
                        List<Feed> feeds = categoryFeeds[category];
                        feeds.Sort((a, b) => String.Compare(a.Name, b.Name));
                        foreach (Feed feed in feeds)
                        {
                            categoryListings.Add(new FeedItemViewModel(feed));
                        }
                        categoryListings.Add(new SpacerViewModel() { SpacerHeight = NavigationViewModel.NavSpacerHeight });
                    }
                }

                if (categoryFeeds.ContainsKey(noCategoryKey))
                {
                    categoryListings.Add(new CategoryViewModel() { DisplayName = "Other", Type = CategoryViewModel.CategoryType.Other });
                    List<Feed> feeds = categoryFeeds[noCategoryKey];
                    feeds.Sort((a, b) => String.Compare(a.Name, b.Name));
                    foreach (Feed feed in feeds)
                    {
                        categoryListings.Add(new FeedItemViewModel(feed));
                    }
                    categoryListings.Add(new SpacerViewModel() { SpacerHeight = NavigationViewModel.NavSpacerHeight });
                }
            }

            return categoryListings;
        }

        private void LstBxCategorySelector_Loaded(object sender, RoutedEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            if (listbox != null)
            {
                if (listbox.ItemsSource == null) listbox.ItemsSource = InitCategoryListings();
                listbox.SelectedItem = null;
            }
        }

        private void LstBxCategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            if (listbox != null && e.AddedItems.Count > 0)
            {
                listbox.SelectedItem = null;
            }
        }

        private void ListItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                if (element.DataContext is CategoryViewModel)
                {
                    CategoryViewModel category = (CategoryViewModel)element.DataContext;
                    if (PopupListSelector != null)
                    {
                        PopupListSelector.IsOpen = false;
                        AddCategoryCluster(category.DisplayName, category.Info.Category);
                    }
                }
                else if (element.DataContext is FeedItemViewModel)
                {
                    Feed feed = ((FeedItemViewModel)element.DataContext).Feed;
                    if (PopupListSelector != null)
                    {
                        PopupListSelector.IsOpen = false;
                        AddFeedCluster(feed.Name, feed.Id);
                    }
                }
            }
        }

        private void PopupListSelector_Opened(object sender, object e)
        {
            Popup popup = sender as Popup;
            SbListSelectorPopIn.Begin();
        }

        private void BtnAddCluster_Click(object sender, RoutedEventArgs e)
        {
            if (PopupListSelector != null)
            {
                PopupListSelector.IsOpen = true;
                ResizeAddPopup();
            }
        }

        private async Task InitClusters()
        {
            if (_startItems.Count == _defaultStartItemCount + AppendixItemCount) // no clusters present
            {
                List<StartClusterViewModel> storedClusters = await ClusterHelper.GetStoredClusters();
                foreach (StartClusterViewModel cluster in storedClusters)
                {
                    _startItems.Insert(_startItems.Count - AppendixItemCount, cluster);
                    cluster.IsLoading = true;
                }
            }
        }

        private async void RefreshClusters()
        {
            StartClusterViewModel cluster;
            for (int i = 0; i < _startItems.Count; i++)
            {
                cluster = _startItems[i] as StartClusterViewModel;
                if (cluster != null) await cluster.InitCluster();
            }
        }

        private void ClearClusters()
        {
            List<StartItemBase> removeItems = new List<StartItemBase>();
            foreach (StartItemBase item in _startItems)
            {
                if (item is StartClusterViewModel) removeItems.Add(item);
            }
            foreach (StartItemBase item in removeItems)
            {
                _startItems.Remove(item);
            }
        }

        private async void AddCategoryCluster(String header, String categoryKey)
        {
            if (!String.IsNullOrEmpty(categoryKey))
            {
                StartClusterViewModel cluster = new StartClusterViewModel();
                cluster.Header = header;
                cluster.Category = categoryKey;
                ClusterHelper.StoreCategoryCluster(cluster);
                _startItems.Insert(_startItems.Count - AppendixItemCount, cluster);
                ClusterHelper.UpdateClusterOrder(GetClusterOrder());
                await cluster.InitCluster();
            }
        }

        private async void AddFeedCluster(String header, Guid feedId)
        {
            StartClusterViewModel cluster = new StartClusterViewModel();
            cluster.Header = header;
            cluster.FeedId = feedId;
            ClusterHelper.StoreFeedCluster(cluster);
            _startItems.Insert(_startItems.Count - AppendixItemCount, cluster);
            ClusterHelper.UpdateClusterOrder(GetClusterOrder());
            await cluster.InitCluster();
        }

        private List<String> GetClusterOrder()
        {
            List<String> clusterOrder = new List<string>();
            StartClusterViewModel cluster;
            for (int i = 1; i < _startItems.Count - AppendixItemCount; i++)
            {
                if (_startItems[i] is StartClusterViewModel)
                {
                    cluster = (StartClusterViewModel)_startItems[i];
                    if (!String.IsNullOrEmpty(cluster.Category)) clusterOrder.Add(cluster.Category);
                    else if (cluster.FeedId != null) clusterOrder.Add(cluster.FeedId.ToString());
                }
            }
            return clusterOrder;
        }

        private void ClusterHelper_ClusterRemoved(StartClusterViewModel cluster)
        {
            if (cluster != null)
            {
                _startItems.Remove(cluster);
                ClusterHelper.UpdateClusterOrder(GetClusterOrder());
            }
        }

        private void LatestArticles_HeroSelected(StartNewsItemContainer obj)
        {
            if (obj != null)
            {
                SaveCustomHeroId(obj.NewsItem.Id.ToString());

                int index = _latestArticlesVm.Items.IndexOf(obj);
                if (index > -1)
                {
                    _latestArticlesVm.Items.RemoveAt(index);
                    _latestArticlesVm.InserItem(index, new StartNewsItemContainer(_heroArticleVm.Article));
                    _heroArticleVm.Article = obj.NewsItem;
                }
            }
        }

        private Guid? LoadCustomHeroId()
        {
            ApplicationDataContainer settings = UserHelper.Instance.GetUserContainer(false);
            if (settings.Values.ContainsKey(CustomHeroIdKey)) return new Guid((String)settings.Values[CustomHeroIdKey]);
            else return null;
        }

        private void SaveCustomHeroId(String id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationDataContainer settings = UserHelper.Instance.GetUserContainer(false);
                settings.Values[CustomHeroIdKey] = id;
            }
        }

        private async void FirstLaunchControl_Completed(object obj)
        {
            FirstLaunchView control = obj as FirstLaunchView;
            if (control != null)
            {
                GrdMainLogo.Visibility = Windows.UI.Xaml.Visibility.Visible;
                GrdFirstLaunch.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                await control.InitialiseUserSelectedFeeds();
                await InitMainPage();
            }
        }

        private void ShareHandler(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs e)
        {
            e.Request.FailWithDisplayText("Go to an article you'd like to share and try again.");
        }

        private static bool _requireCategoryRefresh;
        public static bool RequireCategoryRefresh
        {
            get { return _requireCategoryRefresh; }
            set { _requireCategoryRefresh = value; }
        }

        private static bool _requireAllRefresh;
        public static bool RequireAllRefresh
        {
            get { return _requireAllRefresh; }
            set { _requireAllRefresh = value; }
        }

        private async void AppBarRefresh_Click(object sender, RoutedEventArgs e)
        {
            _forceTileUpdate = true;
            await Refresh();
        }

        public async Task Refresh()
        {
            LstVwMain.ItemsSource = null;
            PrgRngLoadingMain.IsActive = true;
            await UserHelper.Instance.LoadUser(true);
            await LoadViewModels();
            LstVwMain.ItemsSource = _startItems;
            PrgRngLoadingMain.IsActive = false;
        }

        private async void UserChanged(object obj)
        {
            ClusterHelper.ClearAllClusters();
            LiveTileHelper.ResetMainTile();
            await Refresh();
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

        private bool _forceTileUpdate = false;
        private const int LiveTileArticleCount = 3;

        private void UpdateLiveTileArticles(IList<NewsItem> items)
        {
            if (items.Count > 0)
            {
                String latestId = items[0].Id.ToString();
                if (_forceTileUpdate || LiveTileHelper.RequireMainTileUpdate(latestId))
                {
                    _forceTileUpdate = false;
                    LiveTileHelper.EnableTileQueue(true);
                    LiveTileHelper.ClearMainTile();
                    for (int i = 0, count = 0; i < items.Count && count < LiveTileArticleCount; i++)
                    {
                        if (items[i].HasImage)
                        {
                            LiveTileHelper.UpdateVideoTile(items[i]);
                            count++;
                        }
                    }
                    LiveTileHelper.UpdateMainTileLatestId(latestId);
                }
            }
        }

        private void AppBarClearRoaming_Click(object sender, RoutedEventArgs e)
        {
            UserHelper.Instance.ClearRoamingData();
        }

        private void _startItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems.Count > 0 && !semanticZoomControl.IsZoomedInViewActive && !_initialRepositionSections)
            {
                if (e.NewItems[0] is StartHeroArticle)
                {
                    CannotReorderHero(e.NewStartingIndex);
                }
                else if (e.NewItems[0] is StartAddViewModel)
                {
                    CannotReorderAdd(e.NewStartingIndex);
                }
                else
                {
                    SaveCustomIndices();
                    ClusterHelper.UpdateClusterOrder(GetClusterOrder());
                }
            }
        }

        private async void CannotReorderHero(int newIndexRequest)
        {
            await Task.Delay(1);
            _startItems.Move(newIndexRequest, 0);
            MessageDialog dialog = new MessageDialog("Cannot move the hero section", "Oops!");
            await dialog.ShowAsync();
        }

        private async void CannotReorderAdd(int newIndexRequest)
        {
            await Task.Delay(1);
            _startItems.Move(newIndexRequest, _startItems.Count - AppendixItemCount);
            MessageDialog dialog = new MessageDialog("Cannot move the add cluster section", "Oops!");
            await dialog.ShowAsync();
        }

        private bool _initialRepositionSections = false;

        /// <summary>
        /// Repositions the standard sections if they have been customised.
        /// </summary>
        private void RepositionSections()
        {
            if (_startItems.Count > 0)
            {
                _initialRepositionSections = true;
                ApplicationDataContainer settings = UserHelper.Instance.GetUserContainer(true);
                int latestArticleIndex = _startItems.IndexOf(_latestArticlesVm);
                if (settings.Values.ContainsKey(LatestArticlesIndexKey))
                {
                    int newIndex = (int)settings.Values[LatestArticlesIndexKey];
                    if (newIndex != latestArticleIndex) _startItems.Move(latestArticleIndex, newIndex);
                }

                int sourcesIndex = _startItems.IndexOf(_sourcesVm);
                if (settings.Values.ContainsKey(SourcesIndexKey))
                {
                    int newIndex = (int)settings.Values[SourcesIndexKey];
                    if (newIndex != sourcesIndex) _startItems.Move(sourcesIndex, newIndex);
                }
                _initialRepositionSections = false;
            }
        }

        private void SaveCustomIndices()
        {
            if (_startItems.Count > 0)
            {
                ApplicationDataContainer settings = UserHelper.Instance.GetUserContainer(true);
                int latestArticleIndex = _startItems.IndexOf(_latestArticlesVm);
                int sourcesIndex = _startItems.IndexOf(_sourcesVm);
                settings.Values[LatestArticlesIndexKey] = latestArticleIndex;
                settings.Values[SourcesIndexKey] = sourcesIndex;
            }
        }

        private void AdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            ThreadHelper.CheckBeginInvokeOnUI(() => _advertisingVm.ShowFallbackAd = true);
        }

        private void BtnFallbackAd_Click(object sender, RoutedEventArgs e)
        {
            _advertisingVm.ExecuteFallbackAd();
        }

        private void SetViewMode(Size size)
        {
            if (size.Width > 0 && size.Height > 0)
            {
                if (size.Height > size.Width)
                {
                    if (size.Width < DynamicThreshold)
                    {
                        LstVwMain.Style = this.Resources["MainListViewStyleDynamic"] as Style;
                        //LstVwMain.Style = this.Resources["MainListViewStylePortrait"] as Style;
                    }
                    else
                    {
                        LstVwMain.Style = this.Resources["MainListViewStylePortrait"] as Style;
                    }
                }
                else
                {
                    LstVwMain.Style = this.Resources["MainListViewStyle"] as Style;
                }
            }
        }

        private void BtnAddSources_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            parameters.Add(BrowsePage.NavParamAddSourceKey, true);
            App.Navigate(typeof(BrowsePage), parameters);
        }

    } // end of class
}
