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
            List<int> noImageIndices = new List<int>();
            int index = 0;
            foreach (NewsItem item in news.News)
            {
                if (!item.HasImage) noImageIndices.Add(index);
                containerItems.Add(new StartNewsItemContainer(item));
                index++;
            }

            int layoutChoice = _random.Next() % LayoutCount;
            if (noImageIndices.Contains(0)) layoutChoice = 1;
            else if (layoutChoice == _previousLayout) layoutChoice = (layoutChoice + 1) % 2;
            switch (layoutChoice)
            {
                case 0:
                    InitLayout_0(containerItems);
                    break;
                case 1:
                    InitLayout_1(containerItems);
                    break;
                default:
                    InitLayout_0(containerItems);
                    layoutChoice = 0;
                    break;
            }
            _previousLayout = layoutChoice;
        }

        private void InitLayout_0(List<StartNewsItemContainer> items)
        {
            int index = 0;
            int displayCount = BaseDisplayCount + ExtraRows;
            bool showImageFlag = false;
            foreach (StartNewsItemContainer i in items)
            {
                if (index == 0)
                {
                    i.IsMain = true;
                    i.WidthSpan = 2;
                    i.HeightSpan = 2;
                }
                else if (index < 3)
                {
                    i.WidthSpan = 1;
                    i.HeightSpan = 1;
                }
                else
                {
                    i.WidthSpan = 2;
                    i.HeightSpan = 1;
                    i.ShowImage = showImageFlag;
                    showImageFlag = !showImageFlag;
                }
                Items.Add(i);
                index++;
                if (index >= displayCount) break;
            }
        }

        private void InitLayout_1(List<StartNewsItemContainer> items)
        {
            int index = 0;
            int displayCount = BaseDisplayCount + ExtraRows;
            bool showImageFlag = false;
            foreach (StartNewsItemContainer i in items)
            {
                if (index == 0)
                {
                    i.IsMain = true;
                    i.WidthSpan = 2;
                    i.HeightSpan = 1;
                    i.ShowImage = false;
                }
                else if (index == 1)
                {
                    i.WidthSpan = 2;
                    i.HeightSpan = 1;
                    i.ShowImage = false;
                }
                else if (index < 4)
                {
                    i.WidthSpan = 1;
                    i.HeightSpan = 1;
                }
                else if (index == 4)
                {
                    i.WidthSpan = 2;
                    i.HeightSpan = 1;
                    i.ShowImage = false;
                }
                else
                {
                    i.WidthSpan = 2;
                    i.HeightSpan = 1;
                    i.ShowImage = showImageFlag;
                    showImageFlag = !showImageFlag;
                }
                Items.Add(i);
                index++;
                if (index >= displayCount + 1) break;
            }
        }

        private static int _extraRows = 0;
        public static int ExtraRows
        {
            get { return _extraRows; }
            set { _extraRows = value; }
        }

    } // end of class
}
