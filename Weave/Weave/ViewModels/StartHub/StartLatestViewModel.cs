﻿using System;
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

        private NewsItem _zoomedArticle = null;
        public NewsItem ZoomedArticle
        {
            get { return _zoomedArticle; }
            set { SetProperty(ref _zoomedArticle, value); }
        }

        public void InitItems(List<StartNewsItemContainer> newsItems)
        {
            Items.Clear();
            int index = 0;

            // ensure first item has an image
            if (newsItems.Count > 1 && !newsItems[0].NewsItem.HasImage)
            {
                int imageItemIndex = -1;
                for (int i = 1; i < newsItems.Count; i++)
                {
                    if (newsItems[i].NewsItem.HasImage)
                    {
                        imageItemIndex = i;
                    }
                }

                if (imageItemIndex > -1)
                {
                    StartNewsItemContainer temp = newsItems[imageItemIndex];
                    newsItems.RemoveAt(imageItemIndex);
                    newsItems.Insert(0, temp);
                }
            }

            foreach (StartNewsItemContainer item in newsItems)
            {
                PrepareItem(index, item);

                Items.Add(item);

                if (ZoomedArticle == null && item.NewsItem.HasImage) ZoomedArticle = item.NewsItem;

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
