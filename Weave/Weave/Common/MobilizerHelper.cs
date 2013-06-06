using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.ViewModels;

namespace Weave.Common
{
    public static class MobilizerHelper
    {
        private static Formatter _formatter = new Formatter();
        private static Client _client = new Client();

        public static async Task<String> GetMobilizedHtml(NewsItem item)
        {
            String result = null;

            try
            {
                MobilizerResult content = await _client.GetAsync(item.Link);
                result = await _formatter.CreateHtml(item.FormattedForMainPageSourceAndDate, item.Title, item.Link, content.content, "#000000", "#FFFFFF", "Segoe UI", "12", "#0000FF");
            }
            catch (Exception e)
            {
                App.LogError("Error getting mobilized html", e);
                result = null;
            }

            return result;
        }

    } // end of class
}
