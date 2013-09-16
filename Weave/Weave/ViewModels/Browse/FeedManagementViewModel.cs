using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;
using Weave.FeedLibrary;

namespace Weave.ViewModels.Browse
{
    public class FeedManagementViewModel : BindableBase
    {
        public event Action<object, Feed> FeedAdded;
        public const String FeedsUrl = "http://weave.blob.core.windows.net/settings/masterfeeds.xml";

        private ExpandedLibrary _feedLibrary;
        private Dictionary<String, List<FeedItemViewModel>> _categoryFeedMap;
        private List<String> _categories;
        private List<FeedItemViewModel> _categoryItems;

        private FeedSearchService.FeedSearchService _searchService;

        public FeedManagementViewModel()
        {
            _feedLibrary = new ExpandedLibrary(FeedsUrl + "?xsf=" + (new Random()).Next(123, 978));
            _searchService = new FeedSearchService.FeedSearchService();
        }

        public async Task InitFeeds()
        {
            if (!IsLoadingCategories)
            {
                IsLoadingCategories = true;
                try
                {
                    List<Feed> feeds = await _feedLibrary.Feeds.Value;
                    _categoryFeedMap = BuildCategoryCollection(feeds);
                    if (_categoryFeedMap != null)
                    {
                        _categories = _categoryFeedMap.Keys.ToList();
                        _categories.Sort();
                        OnPropertyChanged("Categories");
                        IsInitialised = true;
                    }
                }
                catch (Exception)
                {
                    _categoryFeedMap = null;
                }
                IsLoadingCategories = false;
            }
        }

        private void Reset()
        {
            _categories = null;
            _categoryItems = null;
            if (_categoryFeedMap != null)
            {
                _categoryFeedMap.Clear();
                _categoryFeedMap = null;
            }
            IsInitialised = false;
        }

        public static Dictionary<String, List<FeedItemViewModel>> BuildCategoryCollection(IEnumerable<Feed> feeds)
        {
            Dictionary<String, List<FeedItemViewModel>> collection = new Dictionary<string, List<FeedItemViewModel>>();
            String key;
            foreach (Feed f in feeds)
            {
                key = f.Category == null ? "" : f.Category;
                if (!collection.ContainsKey(key)) collection[key] = new List<FeedItemViewModel>();
                FeedItemViewModel vm = new FeedItemViewModel(f);
                vm.IsAdded = UserHelper.Instance.IsFeedAdded(f.Name);
                collection[key].Add(vm);
            }
            return collection;
        }

        public void SelectCategory(String category)
        {
            if (IsInitialised)
            {
                if (!String.IsNullOrEmpty(category) && _categoryFeedMap.ContainsKey(category))
                {
                    Header = category;
                    _categoryItems = _categoryFeedMap[category];
                    OnPropertyChanged("CategoryItems");
                }
                else if (_categoryItems != null)
                {
                    Header = null;
                    _categoryItems = null;
                    OnPropertyChanged("CategoryItems");
                }
            }
        }

        public async Task Search(String query)
        {
            if (!String.IsNullOrEmpty(query))
            {
                if (_categoryItems != null)
                {
                    _categoryItems = null;
                    OnPropertyChanged("CategoryItems");
                }
                Header = '"' + query + '"';
                IsLoading = true;
                FeedSearchService.FeedApiResult result = await _searchService.SearchForFeedsMatching(query, System.Threading.CancellationToken.None);
                if (result.responseStatus == "200")
                {
                    List<FeedItemViewModel> resultsVm = new List<FeedItemViewModel>();
                    Feed feed;
                    foreach (FeedSearchService.Entry item in result.responseData.entries)
                    {
                        feed = new Feed();
                        feed.ArticleViewingType = ArticleViewingType.Mobilizer;
                        item.Sanitize();
                        feed.Name = item.title;
                        feed.Uri = item.url;
                        resultsVm.Add(new FeedItemViewModel(feed));
                    }
                    _categoryItems = resultsVm;
                    OnPropertyChanged("CategoryItems");
                }
                IsLoading = false;
            }
        }

        public IEnumerable<String> Categories
        {
            get { return _categories; }
        }

        public IEnumerable<FeedItemViewModel> CategoryItems
        {
            get { return _categoryItems; }
        }
        
        private String _header;
        public String Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private bool _initialised = false;
        public bool IsInitialised
        {
            get { return _initialised; }
            set { SetProperty(ref _initialised, value); }
        }

        private bool _isLoadingCategories;
        public bool IsLoadingCategories
        {
            get { return _isLoadingCategories; }
            set { SetProperty(ref _isLoadingCategories, value); }
        }

        public async void AddFeedToCategory(String category, FeedItemViewModel feed)
        {
            if (!String.IsNullOrEmpty(category) && feed != null)
            {
                feed.Feed.Category = category;
                Feed addedFeed = await UserHelper.Instance.AddFeed(feed.Feed);
                if (FeedAdded != null) FeedAdded(this, addedFeed);
                feed.IsAdded = true;
            }
        }

        public async void RemoveFeed(FeedItemViewModel feed)
        {
            if (feed != null)
            {
                await UserHelper.Instance.RemoveFeed(feed.Feed);
                Reset();
            }
        }

        public async void RemoveCategory(CategoryViewModel category, List<FeedItemViewModel> feeds)
        {
            if (category != null)
            {
                List<Feed> remove = new List<Feed>();
                foreach (FeedItemViewModel vm in feeds) remove.Add(vm.Feed);
                await UserHelper.Instance.RemoveCategoryFeeds(category.Info.Category, remove);
                Reset();
            }
        }

    } // end of class
}
