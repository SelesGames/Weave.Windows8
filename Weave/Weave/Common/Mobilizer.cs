using SelesGames.Rest.JsonDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.Common
{
    public class MobilizerResult
    {
        public string author { get; set; }
        public string content { get; set; }
        public string date_published { get; set; }
        public string domain { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string word_count { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1}", title, domain);
        }
    }

    public class MobilizerClient
    {
        const string R_URL_TEMPLATE = "http://mobilizer.cloudapp.net/ipf?url={0}";

        public static Task<MobilizerResult> GetAsync(string url)
        {
            var client = new JsonDotNetRestClient();
            var encodedUrl = Uri.EscapeDataString(url);
            var fUrl = string.Format(R_URL_TEMPLATE, encodedUrl);
            return client
                .GetAsync<MobilizerResult>(fUrl, System.Threading.CancellationToken.None);
        }
    }
}
