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

        private const String FeedsUrl = "http://weave.blob.core.windows.net/settings/masterfeeds.xml";

        private ExpandedLibrary _feedLibrary;
        private Dictionary<String, List<FeedItemViewModel>> _categoryFeedMap;
        private List<String> _categories;
        private List<FeedItemViewModel> _categoryItems;

        public FeedManagementViewModel()
        {
            _feedLibrary = new ExpandedLibrary(FeedsUrl + "?xsf=" + (new Random()).Next(123, 978));
        }

        public async Task InitFeeds()
        {
            if (!IsLoading)
            {
                IsLoading = true;
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
                IsLoading = false;
            }
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
                    _categoryItems = _categoryFeedMap[category];
                    OnPropertyChanged("CategoryItems");
                }
                else if (_categoryItems != null)
                {
                    _categoryItems = null;
                    OnPropertyChanged("CategoryItems");
                }
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

    } // end of class
}
