using SelesGames.Rest;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Weave.Identity.Service.Contracts;

namespace Weave.Identity.Service.Client
{
    public class ServiceClient : IIdentityService
    {
        const string SERVICE_URL = "http://weave-identity.cloudapp.net/api/identity";

        public async Task<DTOs.IdentityInfo> GetUserById(Guid userId)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("userId", userId)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> SyncFacebook(Guid userId, string facebookToken)
        {
            var url = new UriBuilder(SERVICE_URL + "/sync")
                .AddParameter("userId", userId)
                .AddParameter("facebookToken", facebookToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> SyncTwitter(Guid userId, string twitterToken)
        {
            var url = new UriBuilder(SERVICE_URL + "/sync")
                .AddParameter("userId", userId)
                .AddParameter("twitterToken", twitterToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> SyncMicrosoft(Guid userId, string microsoftToken)
        {
            var url = new UriBuilder(SERVICE_URL + "/sync")
                .AddParameter("userId", userId)
                .AddParameter("microsoftToken", microsoftToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> SyncGoogle(Guid userId, string googleToken)
        {
            var url = new UriBuilder(SERVICE_URL + "/sync")
                .AddParameter("userId", userId)
                .AddParameter("googleToken", googleToken)
                .ToString();

            return await GetIdentityInfo(url);
        }




        #region Private Helper methods

        async Task<DTOs.IdentityInfo> GetIdentityInfo(string url)
        {
            try
            {
                return await CreateClient().GetAsync<DTOs.IdentityInfo>(url, CancellationToken.None);
            }
            catch (WebException responseException)
            {
                var response = responseException.Response as HttpWebResponse;
                if (response == null)
                    throw responseException;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NoMatchingUserException();
                else
                    throw responseException;
            }
        }

        RestClient CreateClient()
        {
            //return new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = true };
            return new SelesGames.Rest.JsonDotNet.JsonDotNetRestClient { UseGzip = false };
        }

        #endregion
    }
}
