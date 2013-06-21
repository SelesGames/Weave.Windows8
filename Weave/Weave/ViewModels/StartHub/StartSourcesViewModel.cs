using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartSourcesViewModel : StartItemBase
    {
        public const int BaseDisplayCount = 7;

        public struct SourceListing
        {
            public String Display { get; set; }
            public String Key { get; set; }
        }

        private ObservableCollection<SourceListing> _items = new ObservableCollection<SourceListing>();
        public ObservableCollection<SourceListing> Items
        {
            get { return _items; }
        }

        private static int _extraRows = 0;
        public static int ExtraRows
        {
            get { return _extraRows; }
            set { _extraRows = value; }
        }

        public override void OnItemClick(object item)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            if (item is SourceListing) parameters[BrowsePage.NavParamSelectedCategoryKey] = ((SourceListing)item).Key;
            App.Navigate(typeof(BrowsePage), parameters);
        }

        public void InitSources()
        {
            Dictionary<String, List<Feed>> categoryFeeds = UserHelper.Instance.CategoryFeeds;
            if (categoryFeeds != null)
            {
                int displayCount = BaseDisplayCount + ExtraRows;
                String category;
                List<String> orderedKeys = new List<string>(categoryFeeds.Keys.OrderBy(s => s));
                for (int i = 0; i < orderedKeys.Count && i <= displayCount; i++)
                {
                    category = orderedKeys[i];
                    if (!String.IsNullOrEmpty(category))
                    {
                        Items.Add(new StartSourcesViewModel.SourceListing() { Display = category, Key = category });
                    }
                }
            }
        }

    } // end of class
}
