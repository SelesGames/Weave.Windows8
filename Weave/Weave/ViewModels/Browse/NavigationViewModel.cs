using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.Browse
{
    public class NavigationViewModel : BindableBase
    {
        private ObservableCollection<object> _items = new ObservableCollection<object>();
        public ObservableCollection<object> Items
        {
            get { return _items; }
        }

        public String InitialSelectedCategory { get; set; }
        public Guid? InitialSelectedFeed { get; set; }

        private Dictionary<CategoryViewModel, List<FeedItemViewModel>> _cateogyrFeedsMap = new Dictionary<CategoryViewModel,List<FeedItemViewModel>>();

        public const int NavSpacerHeight = 40;
        public const int DefaultInitialSelection = 2;

        public int Initialise()
        {
            int initialSelection = DefaultInitialSelection;
            _items.Add(new CategoryViewModel() { DisplayName = "All News", Info = new CategoryInfo() { Category = "All News" }, Type = CategoryViewModel.CategoryType.All });
            _items.Add(new SpacerViewModel() { SpacerHeight = (NavSpacerHeight / 2) });
            _items.Add(new CategoryViewModel() { DisplayName = "Latest News", Type = CategoryViewModel.CategoryType.Latest });
            _items.Add(new SpacerViewModel() { SpacerHeight = (NavSpacerHeight / 2) });
            _items.Add(new CategoryViewModel() { DisplayName = "Favorites", Type = CategoryViewModel.CategoryType.Favorites });
            _items.Add(new SpacerViewModel() { SpacerHeight = NavSpacerHeight });

            String noCategoryKey = "";
            Dictionary<String, List<Feed>> categoryFeeds = UserHelper.Instance.CategoryFeeds;
            if (categoryFeeds != null && categoryFeeds.Count > 0)
            {
                List<String> orderedKeys = new List<string>(categoryFeeds.Keys.OrderBy(s => s));
                foreach (String category in orderedKeys)
                {
                    if (!String.Equals(category, noCategoryKey))
                    {
                        if (InitialSelectedCategory != null && String.Equals(InitialSelectedCategory, category, StringComparison.OrdinalIgnoreCase)) initialSelection = _items.Count;
                        CategoryViewModel categoryVm = new CategoryViewModel() { DisplayName = category, Info = new CategoryInfo() { Category = category } };
                        _items.Add(categoryVm);

                        List<FeedItemViewModel> mappedFeeds = new List<FeedItemViewModel>();
                        _cateogyrFeedsMap[categoryVm] = mappedFeeds;
                        FeedItemViewModel feedVm;

                        List<Feed> feeds = categoryFeeds[category];
                        feeds.Sort((a, b) => String.Compare(a.Name, b.Name));
                        int newCount = 0;
                        foreach (Feed feed in feeds)
                        {
                            if (InitialSelectedFeed != null && InitialSelectedFeed.Value == feed.Id) initialSelection = _items.Count;
                            feedVm = new FeedItemViewModel(feed);
                            feedVm.ParentCategory = categoryVm;
                            _items.Add(feedVm);
                            mappedFeeds.Add(feedVm);
                            newCount += feed.NewArticleCount;
                        }
                        categoryVm.NewCount = newCount;
                        _items.Add(new SpacerViewModel() { SpacerHeight = NavSpacerHeight });
                    }
                }

                if (categoryFeeds.ContainsKey(noCategoryKey))
                {
                    _items.Add(new CategoryViewModel() { DisplayName = "Other", Type = CategoryViewModel.CategoryType.Other });
                    List<Feed> feeds = categoryFeeds[noCategoryKey];
                    feeds.Sort((a, b) => String.Compare(a.Name, b.Name));
                    foreach (Feed feed in feeds)
                    {
                        if (InitialSelectedFeed != null && initialSelection == DefaultInitialSelection && InitialSelectedFeed.Value == feed.Id) initialSelection = _items.Count;
                        _items.Add(new FeedItemViewModel(feed));
                    }
                    _items.Add(new SpacerViewModel() { SpacerHeight = NavSpacerHeight });
                }
            }

            return initialSelection;
        }

        public void RemoveFeed(FeedItemViewModel feed)
        {
            CategoryViewModel categoryVm = feed.ParentCategory;
            if (categoryVm != null && _cateogyrFeedsMap.ContainsKey(categoryVm)) _cateogyrFeedsMap[categoryVm].Remove(feed);
            _items.Remove(feed);
        }

        public void RemoveCategory(CategoryViewModel category)
        {
            if (category != null && _cateogyrFeedsMap.ContainsKey(category))
            {
                int categoryIndex = _items.IndexOf(category);
                if (categoryIndex > -1)
                {
                    while (!(_items[categoryIndex] is SpacerViewModel)) _items.RemoveAt(categoryIndex);
                    _items.RemoveAt(categoryIndex);
                }
                _cateogyrFeedsMap.Remove(category);
            }
        }

        public List<FeedItemViewModel> GetCategoryFeeds(CategoryViewModel category)
        {
            if (_cateogyrFeedsMap.ContainsKey(category)) return _cateogyrFeedsMap[category];
            else return null;
        }

        public FeedItemViewModel InsertFeed(Feed feed)
        {
            if (feed != null && !string.IsNullOrEmpty(feed.Category))
            {
                CategoryViewModel categoryVm = GetCategoryVm(feed.Category);
                int categoryIndex;
                int feedIndex;

                if (categoryVm == null)
                {
                    categoryIndex = FindCategoryInsertIndex(feed.Category);
                    feedIndex = 0;

                    categoryVm = new CategoryViewModel() { DisplayName = feed.Category, Info = new CategoryInfo() { Category = feed.Category } };
                    categoryVm.RequiresRefresh = true;
                    _items.Insert(categoryIndex, categoryVm);
                    _cateogyrFeedsMap[categoryVm] = new List<FeedItemViewModel>();

                    _items.Insert(categoryIndex + 1, new SpacerViewModel() { SpacerHeight = NavSpacerHeight });
                }
                else
                {
                    categoryIndex = _items.IndexOf(categoryVm);
                    feedIndex = FindFeedInsertIndex(_cateogyrFeedsMap[categoryVm], feed.Name);
                }

                if (categoryIndex > -1 && feedIndex > -1)
                {
                    FeedItemViewModel feedVm = new FeedItemViewModel(feed);
                    feedVm.ParentCategory = categoryVm;
                    //feedVm.RequiresRefresh = true;
                    _items.Insert(categoryIndex + 1 + feedIndex, feedVm);
                    _cateogyrFeedsMap[categoryVm].Insert(feedIndex, feedVm);
                    return feedVm;
                }
            }
            return null;
        }

        private int FindFeedInsertIndex(IList<FeedItemViewModel> feeds, String name)
        {
            int index = 0;
            if (feeds.Count > 0)
            {
                foreach (FeedItemViewModel f in feeds)
                {
                    if (String.Compare(f.Feed.Name, name, StringComparison.OrdinalIgnoreCase) > -1) break;
                    index++;
                }
            }

            if (index > feeds.Count) return -1;
            return index;
        }

        private CategoryViewModel GetCategoryVm(String category)
        {
            foreach (CategoryViewModel vm in _cateogyrFeedsMap.Keys)
            {
                if (String.Equals(vm.Info.Category, category)) return vm;
            }
            return null;
        }

        private int FindCategoryInsertIndex(String category)
        {
            List<CategoryViewModel> currentCategories = _cateogyrFeedsMap.Keys.ToList();
            currentCategories.Sort((a,b) => String.Compare(a.DisplayName, b.DisplayName));
            CategoryViewModel insertBefore = null;
            foreach (CategoryViewModel vm in _cateogyrFeedsMap.Keys)
            {
                if (String.Compare(vm.Info.Category, category, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    insertBefore = vm;
                    break;
                }
            }
            if (insertBefore != null) return _items.IndexOf(insertBefore);
            else return _items.Count;

        }

        public void ClearFeedNewCount(FeedItemViewModel feed)
        {
            if (feed != null)
            {
                if (feed.ParentCategory != null) feed.ParentCategory.NewCount -= feed.NewCount;
                feed.NewCount = 0;
            }
        }

        public void ClearCategoryNewCount(CategoryViewModel category)
        {
            if (category != null && category.NewCount > 0)
            {
                category.NewCount = 0;
                if (_cateogyrFeedsMap.ContainsKey(category))
                {
                    foreach (FeedItemViewModel feed in _cateogyrFeedsMap[category]) feed.NewCount = 0;
                }
            }
        }

    } // end of class
}
