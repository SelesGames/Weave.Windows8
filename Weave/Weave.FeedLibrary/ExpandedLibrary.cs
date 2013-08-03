using SelesGames.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Weave.ViewModels;

namespace Weave.FeedLibrary
{
    public class ExpandedLibrary
    {
        string libraryUrl;

        public Lazy<Task<List<Feed>>> Feeds { get; private set; }

        public ExpandedLibrary(string libraryUrl)
        {
            this.libraryUrl = libraryUrl;
            Feeds = new Lazy<Task<List<Feed>>>(GetFeedsFromWeb);
        }




        #region Load Feeds and Categories XML files

        async Task<List<Feed>> GetFeedsFromWeb()
        {
            var client = new LinqToXmlRestClient<List<Feed>> { UseGzip = true };
            var feeds = await client.GetAndParseAsync(libraryUrl, Parse, CancellationToken.None);
            return feeds;
        }

        List<Feed> Parse(XElement doc)
        {
            return doc.Descendants("Feed")
                .Select(feed =>
                    new Feed
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        Name = feed.Attribute("Name").ValueOrDefault(),
                        Uri = feed.ValueOrDefault(),
                        ArticleViewingType = ParseArticleViewingType(feed),
                    })
                .ToList();
        }

        ArticleViewingType ParseArticleViewingType(XElement feed)
        {
            var avt = feed.Attribute("ViewType");
            if (avt == null || string.IsNullOrEmpty(avt.Value))
                return ArticleViewingType.Mobilizer;

            var type = avt.Value;

            //if (type.Equals("rss", StringComparison.OrdinalIgnoreCase))
            //    return ArticleViewingType.Local;

            //else if (type.Equals("exrss", StringComparison.OrdinalIgnoreCase))
            //    return ArticleViewingType.LocalOnly;

            if (type.Equals("ieonly", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.InternetExplorerOnly;

            else if (type.Equals("exmob", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.MobilizerOnly;

            else if (type.Equals("ie", StringComparison.OrdinalIgnoreCase))
                return ArticleViewingType.InternetExplorer;

            else
                return ArticleViewingType.Mobilizer;
        }

        #endregion
    }
}
