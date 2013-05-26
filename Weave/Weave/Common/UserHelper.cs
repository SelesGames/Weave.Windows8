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

namespace Weave.Common
{
    public class UserHelper
    {
        private const String CloudUrlPrefix = "http://weave-user.cloudapp.net/api/user/";
        private const String CurrentUserId = "0d13bf82-0f14-475f-9725-f97e5a123d5a";
        private const String UserInfoUrlFormat = CloudUrlPrefix + "info?userId={0}";

        private UserInfo _currentUser;
        private Weave.ViewModels.Contracts.Client.IViewModelRepository _repo;
        private bool _isLoaded;
        private bool _loading;
        private ManualResetEvent _loadingEvent = new ManualResetEvent(true);

        private Dictionary<String, Weave.ViewModels.Feed> _feedIdMap = new Dictionary<string,ViewModels.Feed>();

        private static UserHelper _instance = new UserHelper();
        public static UserHelper Instance
        {
            get { return _instance; }
        }

        private UserHelper()
        {
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

                    _repo = new ViewModels.Repository.StandardRepository(Guid.Parse(CurrentUserId), new Weave.UserFeedAggregator.Client.Client());
                    _currentUser = await _repo.GetUserInfo(false);

                    _loadingEvent.Set();
                    _loading = false;
                    _isLoaded = true;
                }
                else await Task.Run(() => _loadingEvent.WaitOne()); // wait for loading to complete
            }
        }

        public async Task<NewsList> GetCategoryNews(String category, int count)
        {
            return await _currentUser.GetNewsForCategory(category, false, false, 0, count);
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

    } // end of class
}
