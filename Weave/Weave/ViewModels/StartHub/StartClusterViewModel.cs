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

        private ObservableCollection<StartNewsItemContainer> _items = new ObservableCollection<StartNewsItemContainer>();
        public ObservableCollection<StartNewsItemContainer> Items
        {
            get { return _items; }
        }

        public void InitCluster(NewsList news)
        {
            List<StartNewsItemContainer> containerItems = new List<StartNewsItemContainer>();
            Queue<int> noImageIndices = new Queue<int>();
            Queue<int> imageIndices = new Queue<int>();
            int index = 0;
            foreach (NewsItem item in news.News)
            {
                if (!item.HasImage) noImageIndices.Enqueue(index);
                else imageIndices.Enqueue(index);
                containerItems.Add(new StartNewsItemContainer(item));
                index++;
            }

            int layoutChoice = _random.Next() % LayoutCount;
            //if (noImageIndices.Count > 0 && noImageIndices.Peek() == 0) layoutChoice = 1;
            if (layoutChoice == _previousLayout) layoutChoice = (layoutChoice + 1) % 2;
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
            bool showImageFlag = false;

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
                }
                else if (i < 3)
                {
                    if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                    else container = items[noImageIndices.Dequeue()];
                    container.WidthSpan = 1;
                    container.HeightSpan = 1;
                    container.ShowImage = container.NewsItem.HasImage;
                }
                else
                {
                    if (showImageFlag)
                    {
                        if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                        else container = items[noImageIndices.Dequeue()];
                    }
                    else
                    {
                        if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                        else container = items[imageIndices.Dequeue()];
                    }
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = showImageFlag && container.NewsItem.HasImage;
                    showImageFlag = !showImageFlag;
                }
                Items.Add(container);
            }
        }

        private void InitLayout_1(List<StartNewsItemContainer> items, Queue<int> imageIndices, Queue<int> noImageIndices)
        {
            int index = 0;
            int displayCount = BaseDisplayCount + ExtraRows;
            bool showImageFlag = false;

            StartNewsItemContainer container;
            for (int i = 0; i < items.Count && i < displayCount; i++)
            {
                if (i == 0)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.IsMain = true;
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                }
                else if (i == 1)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                }
                else if (i < 4)
                {
                    if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                    else container = items[noImageIndices.Dequeue()];
                    container.WidthSpan = 1;
                    container.HeightSpan = 1;
                    container.ShowImage = container.NewsItem.HasImage;

                }
                else if (i == 4)
                {
                    if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                    else container = items[imageIndices.Dequeue()];
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = false;
                }
                else
                {
                    if (showImageFlag)
                    {
                        if (imageIndices.Count > 0) container = items[imageIndices.Dequeue()];
                        else container = items[noImageIndices.Dequeue()];
                    }
                    else
                    {
                        if (noImageIndices.Count > 0) container = items[noImageIndices.Dequeue()];
                        else container = items[imageIndices.Dequeue()];
                    }
                    container.WidthSpan = 2;
                    container.HeightSpan = 1;
                    container.ShowImage = showImageFlag && container.NewsItem.HasImage;
                    showImageFlag = !showImageFlag;
                }
                Items.Add(container);
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
            parameters[BrowsePage.NavParamSelectedCategoryKey] = Header;
            App.Navigate(typeof(BrowsePage), parameters);
        }

        public override void OnItemClick(object item)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            parameters[BrowsePage.NavParamSelectedCategoryKey] = Header;
            if (item is StartNewsItemContainer)
            {
                parameters[BrowsePage.NavParamSelectionKey] = ((StartNewsItemContainer)item).NewsItem.Id;
            }
            App.Navigate(typeof(BrowsePage), parameters);
        }

    } // end of class
}
