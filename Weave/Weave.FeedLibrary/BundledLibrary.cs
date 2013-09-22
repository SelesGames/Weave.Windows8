using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Weave.ViewModels;

namespace Weave.FeedLibrary
{
    public class BundledLibrary
    {
        System.Xml.XmlReader reader;


        public Lazy<List<Feed>> Feeds { get; private set; }

        public BundledLibrary(System.Xml.XmlReader reader)
        {
            this.reader = reader;
            Feeds = new Lazy<List<Feed>>(GetFeedsFromXmlFile);
        }




        #region Load Feeds and Categories XML files

        List<Feed> GetFeedsFromXmlFile()
        {
            var doc = XDocument.Load(reader);
            var xmlFeeds = doc.Descendants("Feed")
                .Select(feed =>
                    new Feed
                    {
                        Category = feed.Parent.Attribute("Type").ValueOrDefault(),
                        Name = feed.Attribute("Name").ValueOrDefault(),
                        Uri = feed.ValueOrDefault(),
                        IconUrl = feed.Attribute("IconUrl").ValueOrDefault(),
                        ArticleViewingType = ParseArticleViewingType(feed),
                    })
                .ToList();
            return xmlFeeds;
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
