using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Weave.ViewModels;
using Windows.Storage;

namespace Weave.Common
{
    public class UserHelper
    {
        private const String CloudUrlPrefix = "http://weave-user.cloudapp.net/api/user/";
        private const String CurrentUserId = "b41e8972-60cd-43cb-9974-0ec028bedf68";
        private const String UserInfoUrlFormat = CloudUrlPrefix + "info?userId={0}";

        private String _currentUserId = CurrentUserId;

        private UserInfo _currentUser;
        private Weave.ViewModels.Contracts.Client.IViewModelRepository _repo;
        private bool _isLoaded;
        private bool _loading;
        private ManualResetEvent _loadingEvent = new ManualResetEvent(true);

        private ApplicationDataContainer _localSettings;
        private ApplicationDataContainer _roamingSettings;

        private Dictionary<String, Feed> _feedIdMap = new Dictionary<string,ViewModels.Feed>();
        private Dictionary<String, List<Feed>> _categoryFeedMap;

        private static UserHelper _instance = new UserHelper();
        public static UserHelper Instance
        {
            get { return _instance; }
        }

        private UserHelper()
        {
        }

        public Dictionary<String, List<Feed>> CategoryFeeds
        {
            get
            {
                if (_categoryFeedMap == null && _currentUser != null && _currentUser.Feeds != null)
                {
                    _categoryFeedMap = new Dictionary<string, List<Feed>>();
                    String key;
                    foreach (Feed feed in _currentUser.Feeds)
                    {
                        key = feed.Category == null ? "" : feed.Category;
                        if (!_categoryFeedMap.ContainsKey(key))
                        {
                            _categoryFeedMap[key] = new List<Feed>();
                        }
                        _categoryFeedMap[key].Add(feed);
                    }
                }
                return _categoryFeedMap;
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
        }

        public async Task LoadUser()
        {
            if (!_isLoaded && !_loading)
            {
                if (!_loading)
                {
                    _loading = true;
                    _loadingEvent.Reset();

                    _repo = new ViewModels.Repository.StandardRepository(
                        Guid.Parse(_currentUserId), 
                        new Weave.User.Service.Client.Client(),
                        new Weave.Article.Service.Client.ServiceClient());
                    _currentUser = await _repo.GetUserInfo(true);

                    _loadingEvent.Set();
                    _loading = false;
                    _isLoaded = true;
                }
                else await Task.Run(() => _loadingEvent.WaitOne()); // wait for loading to complete

                //Feed feed = new Feed();
                //feed.Id = new Guid("8a26950a-d939-5c90-76e8-3a193f88bff0");
                //await _currentUser.RemoveFeed(feed);

                //feed = new Feed();
                //feed.Uri = "http://www.cnbc.com/id/15837362/devices/rss.xml";
                //feed.Name = "CNBC";
                //feed.ArticleViewingType = ArticleViewingType.Mobilizer;
                //feed.Category = "Business";
                //await _currentUser.AddFeed(feed);
            }
        }

        public async Task<NewsList> GetCategoryNews(String category, int start, int count, EntryType entry)
        {
            return await _currentUser.GetNewsForCategory(category, entry, start, count);
        }

        public async Task<NewsList> GetFeedNews(Guid feedId, int start, int count, EntryType entry)
        {
            return await _currentUser.GetNewsForFeed(feedId, entry, start, count);
        }

        public async Task<List<NewsItem>> GetFavorites(int start, int count)
        {
            return await _currentUser.GetFavorites(start, count);
        }

        public async Task AddFavorite(NewsItem item)
        {
            await _currentUser.AddFavorite(item);
        }

        public async Task RemoveFavorite(NewsItem item)
        {
            await _currentUser.RemoveFavorite(item);
        }

        public List<NewsItem> GetLatestNews()
        {
            return _currentUser.LatestNews;
        }

        public Feed GetFeed(String id)
        {
            Weave.ViewModels.Feed feed = null;

            if (!String.IsNullOrEmpty(id))
            {
                if (_feedIdMap.ContainsKey(id)) feed = _feedIdMap[id];
                else
                {
                    // map id to view model
                    Feed found = FindFeed(id);
                    if (found != null)
                    {
                        _feedIdMap[id] = found;
                    }
                }
            }

            return feed;
        }

        private Feed FindFeed(String id)
        {
            foreach (Feed f in _currentUser.Feeds)
            {
                if (String.Equals(f.Id.ToString(), id))
                {
                    return f;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the settings container for the current user.
        /// </summary>
        /// <param name="roaming">Indicates if the roaming settings should be used.</param>
        /// <returns>The settings container for the current user, or null if no user found.</returns>
        public ApplicationDataContainer GetUserContainer(bool roaming)
        {
            ApplicationDataContainer container = null;
            ApplicationDataContainer settingsContainer = roaming ? RoamingSettings : LocalSettings;

            if (settingsContainer.Containers.ContainsKey(_currentUserId)) container = settingsContainer.Containers[_currentUserId];
            else container = settingsContainer.CreateContainer(_currentUserId, ApplicationDataCreateDisposition.Always);

            return container;
        }

        private ApplicationDataContainer LocalSettings
        {
            get
            {
                if (_localSettings == null) _localSettings = ApplicationData.Current.LocalSettings;
                return _localSettings;
            }
        }

        private ApplicationDataContainer RoamingSettings
        {
            get
            {
                if (_roamingSettings == null) _roamingSettings = ApplicationData.Current.RoamingSettings;
                return _roamingSettings;
            }
        }

        public async Task MarkAsRead(NewsItem item)
        {
            if (_currentUser != null)
            {
                await _currentUser.MarkArticleRead(item);
            }
        }

        public async Task MarkSoftRead(List<NewsItem> items)
        {
            if (_currentUser != null)
            {
                await _currentUser.MarkArticlesSoftRead(items);
            }
        }

    } // end of class
}
