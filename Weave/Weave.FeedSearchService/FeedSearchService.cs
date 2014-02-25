using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Syndication;

namespace Weave.FeedSearchService
{
    public class FeedSearchService
    {
        SmartHttpClient googleSearchClient;//, directSearchClient;

        public FeedSearchService()
        {
            googleSearchClient = new SmartHttpClient(ContentEncoderSettings.Json);
            var jsonFormatter = googleSearchClient.Formatters.First();
            jsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/javascript"));

            //jsonFormatter
            //directSearchClient = new DelegateRestClient(stream =>
            //{
            //    using (var reader = XmlReader.Create(stream))
            //    {
            //        XmlReader feed = SyndicationFeed.Load(reader);
            //        var result = new FeedApiResult
            //        {
            //            responseStatus = "200",
            //            responseData = new ResponseData
            //            {
            //                entries = new List<Entry> 
            //                { 
            //                    new Entry 
            //                    { 
            //                        //url = feedUrl, 
            //                        title = feed.Title.Text, 
            //                        contentSnippet = feed.Description.Text,
            //                    }
            //                }
            //            }
            //        };
            //        reader.Dispose();
            //        return result;
            //    }
            //});
        }

        public async Task<FeedApiResult> SearchForFeedsMatching(string searchString, CancellationToken cancelToken)
        {
            FeedApiResult result;
            try
            {
                if (Uri.IsWellFormedUriString(searchString, UriKind.Absolute))
                    result = await DirectSearchForFeed(searchString, cancelToken);

                else
                    result = await GoogleSearchForFeedsMatching(searchString, cancelToken);
            }
            catch
            {
                result = new FeedApiResult { responseStatus = "999" };
            }
            return result;
        }

        // Call the RSS url directly, extract it's name and description
        async Task<FeedApiResult> DirectSearchForFeed(string feedUrl, CancellationToken cancelToken)
        {
            try
            {
                SyndicationClient client = new SyndicationClient();
                var feedResponse = await client.RetrieveFeedAsync(new Uri(feedUrl));
                FeedApiResult result = new FeedApiResult();
                result.responseStatus = "200";
                result.responseData = new ResponseData()
                {
                    entries = new List<Entry>()
                    {
                        new Entry()
                        {
                            title = feedResponse.Title.Text,
                            url = feedUrl
                        }
                    }
                };
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Search using Google's RSS search service
        async Task<FeedApiResult> GoogleSearchForFeedsMatching(string searchString, CancellationToken cancelToken)
        {
            var url = string.Format(
                "http://ajax.googleapis.com/ajax/services/feed/find?q={0}&v=1.0",
                Uri.EscapeUriString(searchString));

            var result = await googleSearchClient.GetAsync<FeedApiResult>(url, cancelToken);
            return result;
        }
    }
}
