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
        public const int InitialFetchCount = 20;
        private const int NextPageCount = 20;

        public event Action<object> FirstVideoLoaded;

        private HashSet<Guid> _idsAdded = new HashSet<Guid>();
        private int _total;

        private ObservableCollection<NewsItem> _items = new ObservableCollection<NewsItem>();
        public ObservableCollection<NewsItem> Items
        {
            get { return _items; }
        }

        private String _categoryName;
        public String CategoryName
        {
            private get { return _categoryName; }
            set
            {
                SetProperty(ref _categoryName, value);
                if (value != null && FeedId != null) FeedId = null;
            }
        }

        private Guid? _feedId;
        public Guid? FeedId
        {
            private get { return _feedId; }
            set
            {
                SetProperty(ref _feedId, value);
                if (value != null && CategoryName != null) CategoryName = null;
            }
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
            FeedId = null;
            CategoryName = null;
            _total = 0;
            Items.Clear();
            _idsAdded.Clear();
        }

        public async Task LoadInitialData()
        {
            await LoadData(InitialFetchCount);
        }

        public async void LoadNextPage()
        {
            if (HasNextPage) await LoadData(NextPageCount);
        }

        private async Task LoadData(int count)
        {
            IsLoading = true;
            NewsList list = null;
            if (FeedId != null)
            {
                list = await UserHelper.Instance.GetFeedNews(FeedId.Value, _idsAdded.Count, count);
            }
            else if (!String.IsNullOrEmpty(CategoryName))
            {
                list = await UserHelper.Instance.GetCategoryNews(CategoryName, _idsAdded.Count, count);
            }

            if (list != null)
            {
                _total = list.TotalArticleCount;
                foreach (NewsItem item in list.News)
                {
                    AddItem(item);
                }
            }
            IsLoading = false;
        }

        public bool HasNextPage
        {
            get { return _idsAdded.Count < _total; }
        }

        public int FindItemIndexById(Guid id)
        {
            int index = 0;
            foreach (NewsItem news in Items)
            {
                if (news.Id == id) return index;
                index++;
            }
            return -1;
        }

        public NewsItem FindItemById(Guid id)
        {
            foreach (NewsItem news in Items)
            {
                if (news.Id == id) return news;
            }
            return null;
        }

    } // end of class
}
