using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartClusterViewModel : StartItemBase
    {
        public const int BaseDisplayCount = 4;
        private const int LayoutCount = 2;

        private static Random _random = new Random(DateTime.Now.Millisecond);
        private static int _previousLayout = -1;

        public String Header { get; set; }

        public String Category { get; set; }

        public Guid? FeedId { get; set; }

        private ObservableCollection<StartNewsItemContainer> _items = new ObservableCollection<StartNewsItemContainer>();
        public ObservableCollection<StartNewsItemContainer> Items
        {
            get { return _items; }
        }

        private NewsItem _zoomedArticle = null;
        public NewsItem ZoomedArticle
        {
            get { return _zoomedArticle; }
            set { SetProperty(ref _zoomedArticle, value); }
        }

        public async Task InitCluster()
        {
            IsLoading = true;
            NewsList news = null;
            int clusterFetchCount = StartClusterViewModel.BaseDisplayCount + StartClusterViewModel.ExtraRows + 1;
            if (!String.IsNullOrEmpty(Category))
            {
                news = await UserHelper.Instance.GetCategoryNews(Category, 0, clusterFetchCount, EntryType.Peek);
            }
            else if (FeedId != null)
            {
                news = await UserHelper.Instance.GetFeedNews(FeedId.Value, 0, clusterFetchCount, EntryType.Peek);
            }

            if (news != null)
            {
                InitCluster(news);
            }
            IsLoading = false;
        }

        private void InitCluster(NewsList news)
        {
            List<StartNewsItemContainer> containerItems = new List<StartNewsItemContainer>();
            Queue<int> noImageIndices = new Queue<int>();
            Queue<int> imageIndices = new Queue<int>();
            int index = 0;
            NewsItemIcon itemWithIcon;
            foreach (NewsItem item in news.News)
            {
                itemWithIcon = new NewsItemIcon(item);
                if (!item.HasImage) noImageIndices.Enqueue(index);
                else
                {
                    imageIndices.Enqueue(index);
                    if (ZoomedArticle == null) ZoomedArticle = itemWithIcon;
                }
                containerItems.Add(new StartNewsItemContainer(itemWithIcon));
                index++;
            }
            if (ZoomedArticle == null) ZoomedArticle = containerItems[0].NewsItem;

            int layoutChoice = _random.Next() % LayoutCount;
            if (imageIndices.Count == 0) layoutChoice = 1;
            else if (layoutChoice == _previousLayout) layoutChoice = (layoutChoice + 1) % 2;

            // test selection
            //layoutChoice = 0;

            switch (layoutChoice)
            {
                case 0:
                    InitLayout_0(containerItems, imageIndices, noImageIndices);
                    break;
                case 1:
                    InitLayout_1(containerItems, imageIndices, noImageIndices);
                    break;
                default:
                    InitLayout_0(containerItems, imageIndices, noImageIndices);
                    layoutChoice = 0;
                    break;
            }
            _previousLayout = layoutChoice;
        }

        private void InitLayout_0(List<StartNewsItemContainer> items, Queue<int> imageIndices, Queue<int> noImageIndices)
        {
            int displayCount = BaseDisplayCount + ExtraRows;
            bool altContainer = false;

            StartNewsItemContainer container;
            for (int i = 0; i < items.Count && i < displayCount; i++)
            {
                if (i == 0)
                {
                    if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                    else container = items[noImageIndices.Dequeue()];
                    container.IsMain = true;
                    container.WidthSpan = 2;
                    container.HeightSpan = 2;
                    container.ShowImage = container.NewsItem.HasImage;
                    Items.Add(container);
                }
                else if (i < 3)
                {
                    if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                    else container = items[noImageIndices.Dequeue()];
                    container.WidthSpan = 1;
                    container.HeightSpan = 1;
                    container.ShowImage = container.NewsItem.HasImage;
                    Items.Add(container);
                }
                else
                {
                    if (altContainer)
                    {
                        for (int j = 0; j < 2 && i < items.Count && i < displayCount; i++, j++)
                        {
                            if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                            else container = items[noImageIndices.Dequeue()];
                            container.WidthSpan = 1;
                            container.HeightSpan = 1;
                            container.ShowImage = false;
                            Items.Add(container);
                        }
                        altContainer = false;
                    }
                    else
                    {
                        if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                        else container = items[imageIndices.Dequeue()];
                        container.WidthSpan = 2;
                        container.HeightSpan = 1;
                        container.ShowImage = false;
                        Items.Add(container);
                        altContainer = true;
                    }
                }
            }
        }

        private void InitLayout_1(List<StartNewsItemContainer> items, Queue<int> imageIndices, Queue<int> noImageIndices)
        {
            int index = 0;
            int displayCount = BaseDisplayCount + ExtraRows;
            bool altContainer = false;

            StartNewsItemContainer container;
            for (int i = 0; i < items.Count && i < displayCount; i++)
            {
                container = null;
                if (i == 0)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.IsMain = true;
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                    Items.Add(container);
                }
                else if (i == 1)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                    Items.Add(container);
                }
                else if (i < 4)
                {
                    if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                    else container = items[noImageIndices.Dequeue()];
                    container.WidthSpan = 1;
                    container.HeightSpan = 1;
                    container.ShowImage = container.NewsItem.HasImage;
                    Items.Add(container);
                }
                else if (i == 4)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                    Items.Add(container);
                }
                else
                {
                    if (altContainer)
                    {
                        for (int j = 0; j < 2 && i < items.Count && i < displayCount; i++, j++)
                        {
                            if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                            else container = items[noImageIndices.Dequeue()];
                            container.WidthSpan = 1;
                            container.HeightSpan = 1;
                            container.ShowImage = false;
                            Items.Add(container);
                        }
                        altContainer = false;
                    }
                    else
                    {
                        if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                        else container = items[imageIndices.Dequeue()];
                        container.WidthSpan = 2;
                        container.HeightSpan = 1;
                        container.ShowImage = false;
                        Items.Add(container);
                        altContainer = true;
                    }
                }
            }
        }

        private static int _extraRows = 0;
        public static int ExtraRows
        {
            get { return _extraRows; }
            set { _extraRows = value; }
        }

        public override void OnHeaderClick()
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            if (!String.IsNullOrEmpty(Category)) parameters[BrowsePage.NavParamSelectedCategoryKey] = Header;
            else if (FeedId != null) parameters[BrowsePage.NavParamSelectedSourceKey] = FeedId;
            App.Navigate(typeof(BrowsePage), parameters);
        }

        public override void OnItemClick(object item)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();

            if (!String.IsNullOrEmpty(Category)) parameters[BrowsePage.NavParamSelectedCategoryKey] = Header;
            else if (FeedId != null) parameters[BrowsePage.NavParamSelectedSourceKey] = FeedId;

            if (item is StartNewsItemContainer)
            {
                parameters[BrowsePage.NavParamSelectionKey] = ((StartNewsItemContainer)item).NewsItem.Id;
            }
            App.Navigate(typeof(BrowsePage), parameters);
        }

    } // end of class
}
