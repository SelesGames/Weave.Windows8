using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels
{
    public class NewsFeed : BindableBase
    {
        private ObservableCollection<NewsItem> _items = new ObservableCollection<NewsItem>();
        public ObservableCollection<NewsItem> Items
        {
            get { return _items; }
        }

    } // end of class
}
