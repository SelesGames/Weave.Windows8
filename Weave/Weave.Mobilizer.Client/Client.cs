using SelesGames.Rest.JsonDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Client
{
    public class Client
    {
        const string R_URL_TEMPLATE = "http://mobilizer.cloudapp.net/ipf?url={0}";

        public Task<MobilizerResult> GetAsync(string url)
        {
            var client = new JsonDotNetRestClient();
            var encodedUrl = Uri.EscapeDataString(url);
            var fUrl = string.Format(R_URL_TEMPLATE, encodedUrl);
            return client
                .GetAsync<ReadabilityResult>(fUrl, CancellationToken.None)
                .ContinueWith(t => Parse(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        MobilizerResult Parse(ReadabilityResult result)
        {
            return new MobilizerResult
            {
                author = result.author,
                content = result.content,
                date_published = result.date_published,
                domain = result.domain,
                lead_image_url = result.lead_image_url,
                title = result.title,
                url = result.url,
                word_count = result.word_count,
            };
        }
    }
}
