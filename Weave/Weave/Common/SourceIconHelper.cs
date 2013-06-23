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

        private static Dictionary<String, String> _mapping = new Dictionary<string, string>()
        {
            {"http://feeds.guardian.co.uk/theguardian/world/rss", "TheGuardian.jpg"},
            {"http://rss.cnn.com/rss/cnn_world.rss", "CNN.jpg"},
            {"http://www.huffingtonpost.com/feeds/android/world_pulse.xml", "HuffingtonPost.jpg"},
            {"http://www.telegraph.co.uk/news/worldnews/rss", "TheTelegraph.jpg"},
            {"http://sports.espn.go.com/espn/rss/news", "ESPN.jpg"},
            {"http://rss.cnn.com/rss/si_topstories.rss", "SI.jpg"},
            {"http://bleacherreport.com/articles;feed", "BleacherReport.jpg"},
            {"http://feeds.feedburner.com/cnet/NnTv?tag=contentBody.1", "CNET.jpg"},
            {"http://feeds2.feedburner.com/thenextweb", "TNW.jpg"},
            {"http://www.theverge.com/rss/index.xml", "TheVerge.jpg"},
            {"http://feeds.feedburner.com/WinRumors", "WinRumors.jpg"},
            {"http://www.zdnet.com/blog/microsoft/rss", "ZDnet.jpg"},
            {"http://feeds.feedburner.com/wmexperts?format=xml", "WPCentral.jpg"}
        };

        public static String GetIcon(String sourceUrl)
        {
            String path = null;
            if (!String.IsNullOrEmpty(sourceUrl) && _mapping.ContainsKey(sourceUrl)) path = IconFolder + _mapping[sourceUrl];
            return path;
        }

    } // end of class
}
