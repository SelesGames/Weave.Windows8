using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.Common
{
    public static class SourceIconHelper
    {
        private const String IconFolder = "/Assets/SourceIcons/";
        private const String WebIconPrefix = "http://lazywormapps.com/images/weave/";
        private const String DefaultIconPath = "/Assets/SourceIcons/Default.png";

        private static Dictionary<String, String> _mapping = new Dictionary<string, string>()
        {
            {"http://feeds.guardian.co.uk/theguardian/sport/rss", "TheGuardian.png"},
            {"http://rss.cnn.com/rss/cnn_world.rss", "CNN.png"},
            {"http://www.huffingtonpost.com/feeds/android/world_pulse.xml", "HuffingtonPost.png"},
            {"http://www.telegraph.co.uk/news/worldnews/rss", "TheTelegraph.png"},
            {"http://sports.espn.go.com/espn/rss/news", "ESPN.png"},
            {"http://rss.cnn.com/rss/si_topstories.rss", "SI.jpg"},
            {"http://bleacherreport.com/articles;feed", "br.png"},
            {"http://feeds.feedburner.com/cnet/NnTv?tag=contentBody.1", "Cnet.png"},
            {"http://feeds2.feedburner.com/thenextweb", "TNW.png"},
            {"http://www.theverge.com/rss/index.xml", "TheVerge.png"},
            {"http://feeds.feedburner.com/WinRumors", "WinRumors.png"},
            {"http://www.zdnet.com/blog/microsoft/rss", "ZDnet.png"},
            {"http://feeds.feedburner.com/wmexperts?format=xml", "WPCentral.png"},
            {"http://feeds2.feedburner.com/businessinsider", "BI.png"},
            {"http://feeds.marketwatch.com/marketwatch/topstories", "WSJ.png"},
            {"http://feeds.bbci.co.uk/news/world/rss.xml", "BBC.png"},
            {"http://feeds.reuters.com/reuters/businessnews", "Reuters.png"},
            {"http://feeds.feedburner.com/fastcompany/headlines", "FastCompany.png"},
            {"http://rss.nbcsports.msnbc.com/id/3032112/device/rss/rss.xml", "NBCSports.png"},
            {"http://www.cbssports.com/partners/feeds/rss/home_news", "CBSSports.png"},
            {"http://feeds.feedburner.com/techcrunch", "TechCrunch.png"},
            {"http://feeds.mashable.com/mashable", "Mashable.png"},
            {"http://www.polygon.com/rss/index.xml", "Polygon.png"},
            {"http://www.joystiq.com/rss.xml", "Joystiq.png"},
            {"http://feeds.ign.com/ign/games-articles", "IGN.png"},
            {"http://feeds.gawker.com/kotaku/vip", "Kotaku.png"},
            {"http://feeds.bbci.co.uk/sport/0/rss.xml", "BBCSports.png"}
        };

        public static String GetIcon(String sourceUrl)
        {
            String path = DefaultIconPath;
            if (!String.IsNullOrEmpty(sourceUrl) && _mapping.ContainsKey(sourceUrl)) path = IconFolder + _mapping[sourceUrl];
            return path;
        }

        public static String GetWebIcon(String sourceUrl)
        {
            String path = DefaultIconPath;
            if (!String.IsNullOrEmpty(sourceUrl) && _mapping.ContainsKey(sourceUrl)) path = WebIconPrefix + _mapping[sourceUrl];
            return path;
        }

    } // end of class
}
