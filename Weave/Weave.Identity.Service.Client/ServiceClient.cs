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

        public async Task<DTOs.IdentityInfo> GetUserFromFacebookToken(string facebookToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("facebookToken", facebookToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> GetUserFromTwitterToken(string twitterToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("twitterToken", twitterToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> GetUserFromMicrosoftToken(string microsoftToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("microsoftToken", microsoftToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> GetUserFromGoogleToken(string googleToken)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("googleToken", googleToken)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public async Task<DTOs.IdentityInfo> GetUserFromUserNameAndPassword(string username, string password)
        {
            var url = new UriBuilder(SERVICE_URL)
                .AddParameter("username", username)
                .AddParameter("password", password)
                .ToString();

            return await GetIdentityInfo(url);
        }

        public Task Add(DTOs.IdentityInfo user)
        {
            var client = CreateClient();
            return client.PostAsync(SERVICE_URL, user, CancellationToken.None);
        }

        public Task Update(DTOs.IdentityInfo user)
        {
            var client = CreateClient();
            return client.PutAsync(SERVICE_URL, user, CancellationToken.None);
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
