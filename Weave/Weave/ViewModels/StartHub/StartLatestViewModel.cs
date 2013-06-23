using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartLatestViewModel : StartItemBase
    {
        public const int BaseDisplayCount = 4;

        public String Header { get; set; }

        public StartLatestViewModel()
        {
            Header = "Latest articles";
        }

        private ObservableCollection<StartNewsItemContainer> _items = new ObservableCollection<StartNewsItemContainer>();
        public ObservableCollection<StartNewsItemContainer> Items
        {
            get { return _items; }
        }

        private static int _extraRows = 0;
        public static int ExtraRows
        {
            get { return _extraRows; }
            set { _extraRows = value; }
        }

        public void InitItems(List<StartNewsItemContainer> newsItems)
        {
            int index = 0;
            foreach (StartNewsItemContainer item in newsItems)
            {
                PrepareItem(index, item);

                Items.Add(item);
                index++;
            }
        }

        private void PrepareItem(int index, StartNewsItemContainer item)
        {
            if (index == 0)
            {
                item.IsMain = true;
                item.WidthSpan = 2;
                item.HeightSpan = 3;
            }
            else if (index == 1)
            {
                item.WidthSpan = 2;
                item.HeightSpan = 1;
                item.ShowImage = false;
            }
            else
            {
                item.WidthSpan = 1;
                item.HeightSpan = 2;
            }
        }

        public void InserItem(int index, StartNewsItemContainer item)
        {
            PrepareItem(index, item);
            Items.Insert(index, item);
        }

        public override void OnHeaderClick()
        {
            App.Navigate(typeof(BrowsePage));
        }

        public override void OnItemClick(object item)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            if (item is StartNewsItemContainer)
            {
                parameters[BrowsePage.NavParamSelectionKey] = ((StartNewsItemContainer)item).NewsItem.Id;
            }
            App.Navigate(typeof(BrowsePage), parameters);
        }

    } // end of class
}
