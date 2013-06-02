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
        public event Action<object> FirstVideoLoaded;

        private HashSet<Guid> _idsAdded = new HashSet<Guid>();

        private ObservableCollection<NewsItem> _items = new ObservableCollection<NewsItem>();
        public ObservableCollection<NewsItem> Items
        {
            get { return _items; }
        }

        public bool AddItem(NewsItem item)
        {
            if (!_idsAdded.Contains(item.Id))
            {
                _idsAdded.Add(item.Id);
                if (_idsAdded.Count == 1 && FirstVideoLoaded != null) FirstVideoLoaded(this);
                Items.Add(item);
                return true;
            }
            else return false;
        }

        private bool _loading = false;
        /// <summary>
        /// Gets or sets if the feed is being loaded.
        /// </summary>
        public bool IsLoading
        {
            get { return _loading; }
            set { SetProperty(ref _loading, value); }
        }

        public void ClearData()
        {
            Items.Clear();
            _idsAdded.Clear();
        }

    } // end of class
}
