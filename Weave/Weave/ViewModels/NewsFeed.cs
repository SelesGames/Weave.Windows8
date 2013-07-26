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

        public enum FeedType { None, Feed, Category, Favorites };

        private HashSet<Guid> _idsAdded = new HashSet<Guid>();
        private int _total;

        private ObservableCollection<NewsItem> _items = new ObservableCollection<NewsItem>();
        public ObservableCollection<NewsItem> Items
        {
            get { return _items; }
        }

        private FeedType _feedType = FeedType.None;

        public FeedType CurrentFeedType
        {
            get { return _feedType; }
        }

        private object _feedParam;

        public void SetFeedParam(FeedType type, object feedParam)
        {
            _feedType = type;
            _feedParam = feedParam;
        }

        public bool AddItem(NewsItem item)
        {
            if (!_idsAdded.Contains(item.Id))
            {
                _idsAdded.Add(item.Id);
                if (_idsAdded.Count == 1 && FirstVideoLoaded != null) FirstVideoLoaded(this);
                Items.Add(new NewsItemIcon(item));
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
            _feedType = FeedType.None;
            _feedParam = null;
            _total = 0;
            Items.Clear();
            _idsAdded.Clear();
        }

        public async Task LoadInitialData(EntryType entry = EntryType.Peek)
        {
            await LoadData(InitialFetchCount, entry);
        }

        public async void LoadNextPage()
        {
            if (HasNextPage) await LoadData(NextPageCount, EntryType.Peek);
        }

        private async Task LoadData(int count, EntryType entry)
        {
            IsLoading = true;
            NewsList list = null;

            switch (_feedType)
            {
                case FeedType.Feed:
                    if (_feedParam is Guid) list = await UserHelper.Instance.GetFeedNews((Guid)_feedParam, _idsAdded.Count, count, entry);
                    break;
                case FeedType.Category:
                    if (_feedParam is String) list = await UserHelper.Instance.GetCategoryNews((String)_feedParam, _idsAdded.Count, count, entry);
                    break;
                case FeedType.Favorites:
                    list = new NewsList();
                    list.News = await UserHelper.Instance.GetFavorites(_idsAdded.Count, count);
                    break;
                default:
                    break;
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
