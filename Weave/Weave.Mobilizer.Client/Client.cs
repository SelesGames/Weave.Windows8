using SelesGames.Rest.JsonDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.Contracts;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Client
{
    public class Client : IMobilizerService
    {
        const string R_URL_TEMPLATE = "http://mobilizer.cloudapp.net/ipf?url={0}&stripLeadImage=true";

        public async Task<MobilizerResult> Get(string url, bool stripLeadImage = true)
        {
            var client = new JsonDotNetRestClient();
            var encodedUrl = Uri.EscapeDataString(url);
            var fUrl = string.Format(R_URL_TEMPLATE, encodedUrl);

            var result = await client.GetAsync<MobilizerResult>(fUrl, CancellationToken.None);
            return result;
        }

        public Task Post(string url, MobilizerResult article)
        {
            throw new NotImplementedException();
        }
    }
}