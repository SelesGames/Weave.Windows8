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
        private const int DefaultInitialSelection = 2;

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
                            feedVm = new FeedItemViewModel(feed) { ParentCategory = categoryVm };
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
