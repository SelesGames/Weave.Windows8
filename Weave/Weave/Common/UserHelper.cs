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
        public event Action<object> UserChanged;

        //private const String CloudUrlPrefix = "http://weave-user.cloudapp.net/api/user/";
        //private const String CurrentUserId = "b41e8972-60cd-43cb-9974-0ec028bedf68"; // test user
        private const String DefaultUserIdKey = "DefaultUserId";
        private const String LoggedInUserIdKey = "LoggedInUser";
        //private const String UserInfoUrlFormat = CloudUrlPrefix + "info?userId={0}";

        private String _currentUserId = null;

        private UserInfo _currentUser;
        private Weave.ViewModels.Contracts.Client.IViewModelRepository _repo;

        private Identity.Service.Client.ServiceClient _identityClient;
        private Weave.ViewModels.Identity.IdentityInfo _identityInfo;

        private bool _isLoaded;
        private bool _loading;
        private ManualResetEvent _loadingEvent = new ManualResetEvent(true);

        private bool _isLoggedIn;

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
            _repo = new ViewModels.Repository.StandardRepository(
                        new Weave.User.Service.Client.Client(),
                        new Weave.Article.Service.Client.ServiceClient());

            _identityClient = new Identity.Service.Client.ServiceClient();
            _identityInfo = new ViewModels.Identity.IdentityInfo(_identityClient);
            _identityInfo.UserIdChanged += IdentityInfo_UserIdChanged;

            _currentUserId = GetCurrentUser();
        }

        void IdentityInfo_UserIdChanged(object sender, EventArgs e)
        {
            if (_identityInfo.UserId != Guid.Empty)
            {
                String newId = _identityInfo.UserId.ToString();
                if (!String.Equals(newId, _currentUserId))
                {
                    _currentUserId = newId;
                    if (UserChanged != null) UserChanged(this);
                }
            }
        }

        public Dictionary<String, List<Feed>> CategoryFeeds
        {
            get
            {
                if (_categoryFeedMap == null && _currentUser != null && _currentUser.Feeds != null)
                {
                    _categoryFeedMap = BuildCategoryCollection(_currentUser.Feeds);
                }
                return _categoryFeedMap;
            }
        }

        public List<String> GetAvailableCategories()
        {
            Dictionary<String, List<Feed>> categoryFeeds = CategoryFeeds;
            List<String> categories = categoryFeeds.Keys.ToList();
            categories.Sort();
            return categories;
        }

        private Dictionary<String, List<Feed>> BuildCategoryCollection(IEnumerable<Feed> feeds)
        {
            Dictionary<String, List<Feed>> collection = new Dictionary<string, List<Feed>>();
            String key;
            foreach (Feed f in feeds)
            {
                key = f.Category == null ? "" : f.Category;
                if (!collection.ContainsKey(key)) collection[key] = new List<Feed>();
                collection[key].Add(f);
            }
            return collection;
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
        }

        public async Task<bool> LoadUser(bool refresh = false)
        {
            bool success = true;
            if (!_isLoaded || refresh)
            {
                if (!_loading)
                {
                    _loading = true;
                    _loadingEvent.Reset();

                    if (_currentUserId != null)
                    {
                        _currentUser = await _repo.GetUserInfo(Guid.Parse(_currentUserId), true);
                    }
                    else success = false;

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
            return success;
        }

        public async Task LoadIdentity()
        {
            if (_currentUserId != null)
            {
                Guid userId = Guid.Parse(_currentUserId);
                if (_isLoggedIn && _identityInfo.UserId != userId)
                {
                    _identityInfo.UserId = userId;
                    await _identityInfo.LoadFromUserId();
                }
            }
        }

        public async Task<NewsList> GetCategoryNews(String category, int start, int count, EntryType entry)
        {
            try
            {
                return await _currentUser.GetNewsForCategory(category, entry, start, count);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<NewsList> GetFeedNews(Guid feedId, int start, int count, EntryType entry)
        {
            try
            {
                return await _currentUser.GetNewsForFeed(feedId, entry, start, count);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<NewsItem>> GetFavorites(int start, int count)
        {
            return await _currentUser.GetFavorites(start, count);
        }

        public async Task<List<NewsItem>> GetRead(int start, int count)
        {
            return await _currentUser.GetRead(start, count);
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

        public bool IsFeedAdded(String name)
        {
            foreach (Feed f in _currentUser.Feeds)
            {
                if (String.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool _isNewUser = false;

        public String GetCurrentUser()
        {
            String id = null;
            ApplicationDataContainer settingsContainer = RoamingSettings;
            if (settingsContainer.Values.ContainsKey(LoggedInUserIdKey))
            {
                id = settingsContainer.Values[LoggedInUserIdKey] as String;
                _isLoggedIn = true;
            }
            else if (settingsContainer.Values.ContainsKey(DefaultUserIdKey)) id = settingsContainer.Values[DefaultUserIdKey] as String;
            else
            {
                // first start, generate and store new id and create user on cloud
                _isNewUser = true;
                //id = "b41e8972-60cd-43cb-9974-0ec028bedf68";
            }

            return id;
        }

        public bool IsNewUser
        {
            get { return _isNewUser; }
            private set { _isNewUser = value; }
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

        public async Task<Feed> AddFeed(Feed feed)
        {
            if (_currentUser != null)
            {
                await _currentUser.AddFeed(feed);
                Feed addedFeed = _currentUser.Feeds[_currentUser.Feeds.Count - 1];
                _currentUser.GetNewsForFeed(addedFeed.Id, EntryType.ExtendRefresh, 0, 0);
                String category = addedFeed.Category;
                if (category == null) category = "";
                if (!CategoryFeeds.ContainsKey(category)) CategoryFeeds[category] = new List<Feed>();
                CategoryFeeds[category].Add(addedFeed);
                return addedFeed;
            }
            return null;
        }

        public async Task RemoveFeed(Feed feed)
        {
            if (_currentUser != null)
            {
                await _currentUser.RemoveFeed(feed);
                String category = feed.Category;
                if (category == null) category = "";
                if (CategoryFeeds.ContainsKey(category)) CategoryFeeds[category].Remove(feed);
            }
        }

        public async Task RemoveCategoryFeeds(String category, List<Feed> feeds)
        {
            if (_currentUser != null)
            {
                await _currentUser.BatchChange(null, feeds, null);
                if (category == null) category = "";
                if (CategoryFeeds.ContainsKey(category)) CategoryFeeds.Remove(category);
            }
        }

        public async Task InitUserWithFeeds(List<Feed> feeds)
        {
            if (feeds != null && feeds.Count > 0)
            {
                ApplicationDataContainer settingsContainer = RoamingSettings;
                UserInfo newUserInfo = new UserInfo(_repo);
                foreach (Feed f in feeds) newUserInfo.Feeds.Add(f);
                newUserInfo.Id = Guid.NewGuid();
                await newUserInfo.Save();
                _currentUserId = newUserInfo.Id.ToString();
                settingsContainer.Values[DefaultUserIdKey] = _currentUserId;
                IsNewUser = false;
            }
        }

        public async Task CreateSyncUserFromCurrent()
        {
            if (_currentUser != null)
            {
                ApplicationDataContainer settingsContainer = RoamingSettings;

                _identityInfo.UserId = _currentUser.Id;
                await _identityClient.Add(ConvertIdentityVm(_identityInfo));

                _isLoggedIn = true;
                settingsContainer.Values[LoggedInUserIdKey] = _currentUserId;
            }
        }

        public async Task UpdateSyncUser()
        {
            if (_identityInfo.UserId != Guid.Empty)
            {
                await _identityClient.Update(ConvertIdentityVm(_identityInfo));
            }
        }

        private Identity.Service.DTOs.IdentityInfo ConvertIdentityVm(ViewModels.Identity.IdentityInfo info)
        {
            var o = new Identity.Service.DTOs.IdentityInfo
            {
                UserId = info.UserId,
                UserName = info.UserName,
                PasswordHash = info.PasswordHash,
                FacebookAuthToken = info.FacebookAuthToken,
                TwitterAuthToken = info.TwitterAuthToken,
                MicrosoftAuthToken = info.MicrosoftAuthToken,
                GoogleAuthToken = info.GoogleAuthToken,
            };
            return o;
        }

        public void LoadIdentityDTO(Identity.Service.DTOs.IdentityInfo o)
        {
            _identityInfo.UserName = o.UserName;
            _identityInfo.PasswordHash = o.PasswordHash;
            _identityInfo.FacebookAuthToken = o.FacebookAuthToken;
            _identityInfo.TwitterAuthToken = o.TwitterAuthToken;
            _identityInfo.MicrosoftAuthToken = o.MicrosoftAuthToken;
            _identityInfo.GoogleAuthToken = o.GoogleAuthToken;
            _identityInfo.UserId = o.UserId;
        }

        public Weave.ViewModels.Identity.IdentityInfo IdentityInfo
        {
            get { return _identityInfo; }
        }

    } // end of class
}
