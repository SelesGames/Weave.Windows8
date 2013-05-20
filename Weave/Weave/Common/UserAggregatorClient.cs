using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Weave.Common
{
    public class UserAggregatorClient : Weave.UserFeedAggregator.Contracts.IWeaveUserService
    {
        private const String CloudUrlPrefix = "http://weave-user.cloudapp.net/api/user/";
        private const String UserInfoUrlFormat = CloudUrlPrefix + "info?userId={0}";
        private const String FeedUrlFormat = CloudUrlPrefix + "feed?userId={0}";
        private const String NewsUrlFormat = CloudUrlPrefix + "news?userId={0}";

        private async Task<T> RetrieveFromCloud<T>(String url)
        {
            T result = default(T);

            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                WebResponse response = await request.GetResponseAsync();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        JsonSerializer serializer = JsonSerializer.Create();
                        JsonReader jsonReader = new JsonTextReader(reader);
                        result = serializer.Deserialize<T>(jsonReader);
                        jsonReader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                App.LogError("Error retrieving data from: " + url, e);
            }

            return result;
        }

        public Task<UserFeedAggregator.DTOs.ServerOutgoing.UserInfo> AddUserAndReturnUserInfo(UserFeedAggregator.DTOs.ServerIncoming.UserInfo incomingUser)
        {
            throw new NotImplementedException();
        }

        public async Task<UserFeedAggregator.DTOs.ServerOutgoing.UserInfo> GetUserInfo(Guid userId, bool refresh = false)
        {
            String url = String.Format(UserInfoUrlFormat, userId);
            return await RetrieveFromCloud<Weave.UserFeedAggregator.DTOs.ServerOutgoing.UserInfo>(url);
        }

        public async Task<UserFeedAggregator.DTOs.ServerOutgoing.NewsList> GetNews(Guid userId, string category, bool refresh = false, bool markEntry = false, int skip = 0, int take = 10, UserFeedAggregator.DTOs.NewsItemType type = Weave.UserFeedAggregator.DTOs.NewsItemType.Any, bool requireImage = false)
        {
            String url = String.Format(NewsUrlFormat, userId);
            url += String.Format("&category={0}&refresh{1}&markEntry={2}&skip={3}&take={4}&type={5}", category, refresh, markEntry, skip, take, (int)type);
            return await RetrieveFromCloud<UserFeedAggregator.DTOs.ServerOutgoing.NewsList>(url);
        }

        public Task<UserFeedAggregator.DTOs.ServerOutgoing.NewsList> GetNews(Guid userId, Guid feedId, bool refresh = false, bool markEntry = false, int skip = 0, int take = 10, UserFeedAggregator.DTOs.NewsItemType type = Weave.UserFeedAggregator.DTOs.NewsItemType.Any, bool requireImage = false)
        {
            throw new NotImplementedException();
        }

        public Task<UserFeedAggregator.DTOs.ServerOutgoing.FeedsInfoList> GetFeeds(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserFeedAggregator.DTOs.ServerOutgoing.Feed> AddFeed(Guid userId, UserFeedAggregator.DTOs.ServerIncoming.NewFeed feed)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFeed(Guid userId, Guid feedId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFeed(Guid userId, UserFeedAggregator.DTOs.ServerIncoming.UpdatedFeed feed)
        {
            throw new NotImplementedException();
        }

        public Task BatchChange(Guid userId, UserFeedAggregator.DTOs.ServerIncoming.BatchFeedChange changeSet)
        {
            throw new NotImplementedException();
        }

        public Task MarkArticleRead(Guid userId, Guid feedId, Guid newsItemId)
        {
            throw new NotImplementedException();
        }

        public Task MarkArticleUnread(Guid userId, Guid feedId, Guid newsItemId)
        {
            throw new NotImplementedException();
        }

        public Task MarkArticlesSoftRead(Guid userId, List<Guid> newsItemIds)
        {
            throw new NotImplementedException();
        }

        public Task AddFavorite(Guid userId, Guid feedId, Guid newsItemId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFavorite(Guid userId, Guid feedId, Guid newsItemId)
        {
            throw new NotImplementedException();
        }
    } // end of class
}
