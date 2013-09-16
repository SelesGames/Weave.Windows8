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
using Weave.ViewModels.Browse;
using Windows.Storage;

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

        private const String CustomHeroIdKey = "CustomHeroId"; // stores the custom hero article id for showcasing

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

            FirstLaunchControl.Completed += FirstLaunchControl_Completed;
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            ClusterHelper.ClusterRemoved += ClusterHelper_ClusterRemoved;
            Weave.Views.StartHub.LatestArticles.HeroSelected += LatestArticles_HeroSelected;
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            ClusterHelper.ClusterRemoved -= ClusterHelper_ClusterRemoved;
            Weave.Views.StartHub.LatestArticles.HeroSelected -= LatestArticles_HeroSelected;

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

        private void PopupFlyout_Closed(object sender, object e)
        {
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
                await UserHelper.Instance.LoadUser();
                await LoadViewModels();
            }
            LstVwMain.ItemsSource = _startItems;
            PrgRngLoadingMain.IsActive = false;

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
            List<NewsItem> newsItems = UserHelper.Instance.GetLatestNews();
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

            await InitClusters();
            RefreshClusters();

            //StartClusterViewModel cluster = new StartClusterViewModel();
            //cluster.Header = "Comic books";
            //cluster.Category = "comic books";
            //_startItems.Insert(_startItems.Count - 1, cluster);
            //cluster.InitCluster();

            //cluster = new StartClusterViewModel();
            //cluster.Header = "Business";
            //cluster.Category = "business";
            //_startItems.Insert(_startItems.Count - 1, cluster);
            //cluster.InitCluster();

            //cluster = new StartClusterViewModel();
            //cluster.Header = "Cars";
            //cluster.Category = "cars";
            //_startItems.Insert(_startItems.Count - 1, cluster);
            //cluster.InitCluster();

            //cluster = new StartClusterViewModel();
            //cluster.Header = "Anandtech";
            //cluster.FeedId = new Guid("7653d36b-b79c-3a9c-1919-645b48c3ed59");
            //_startItems.Insert(_startItems.Count - 1, cluster);
            //cluster.InitCluster();
        }

        public const double BaseHeight = 768; // base height of design screen
        private const double BinHeight = 280; // extra height to incorporte next bin of results

        private double _expansionFactor;

        private void AdjustForScreenResolution()
        {
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
            if (view != null && _addClusterButton == null)
            {
                _addClusterButton = view.AddButton;
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
                else if (element.DataContext is Feed)
                {
                    Feed feed = (Feed)element.DataContext;
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
            if (_startItems.Count == _defaultStartItemCount + 1) // no clusters present
            {
                List<StartClusterViewModel> storedClusters = await ClusterHelper.GetStoredClusters();
                foreach (StartClusterViewModel cluster in storedClusters)
                {
                    _startItems.Insert(_startItems.Count - 1, cluster);
                    cluster.IsLoading = true;
                }
            }
        }

        private async void RefreshClusters()
        {
            StartClusterViewModel cluster;
            for (int i = _defaultStartItemCount; i < _startItems.Count - 1; i++)
            {
                cluster = _startItems[i] as StartClusterViewModel;
                if (cluster != null) await cluster.InitCluster();
            }
        }

        private void ClearClusters()
        {
            int totalStartCount = _defaultStartItemCount + 1;
            while (_startItems.Count > totalStartCount) _startItems.RemoveAt(_defaultStartItemCount);
        }

        private async void AddCategoryCluster(String header, String categoryKey)
        {
            if (!String.IsNullOrEmpty(categoryKey))
            {
                StartClusterViewModel cluster = new StartClusterViewModel();
                cluster.Header = header;
                cluster.Category = categoryKey;
                ClusterHelper.StoreCategoryCluster(cluster);
                _startItems.Insert(_startItems.Count - 1, cluster);
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
            _startItems.Insert(_startItems.Count - 1, cluster);
            ClusterHelper.UpdateClusterOrder(GetClusterOrder());
            await cluster.InitCluster();
        }

        private List<String> GetClusterOrder()
        {
            List<String> clusterOrder = new List<string>();
            StartClusterViewModel cluster;
            for (int i = _defaultStartItemCount; i < _startItems.Count - 1; i++)
            {
                cluster = _startItems[i] as StartClusterViewModel;
                if (!String.IsNullOrEmpty(cluster.Category)) clusterOrder.Add(cluster.Category);
                else if (cluster.FeedId != null) clusterOrder.Add(cluster.FeedId.ToString());
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

        private void FirstLaunchControl_Completed(object obj)
        {
            GrdFirstLaunch.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UserHelper.Instance.IsNewUser = false;
            InitMainPage();
        }

    } // end of class
}
