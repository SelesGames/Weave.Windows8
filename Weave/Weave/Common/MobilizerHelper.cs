using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                String body = content.content;
                int firstCharIndex = -1;
                StringBuilder sb = new StringBuilder();

                foreach (Match m in Regex.Matches(body, "<p[^>]*>(?<content>.*)</p>", RegexOptions.Singleline))
                {
                    if (m.Success && m.Groups["content"].Success)
                    {
                        Group g = m.Groups["content"];
                        firstCharIndex = FindFirstCharIndex(g.Value);
                        if (firstCharIndex > -1)
                        {
                            firstCharIndex += g.Index;
                            break;
                        }
                    }
                }

                if (firstCharIndex > -1)
                {
                    sb.Append(body.Substring(0, firstCharIndex));
                    sb.Append("<span class=\"first-character\">");
                    sb.Append(body[firstCharIndex]);
                    sb.Append("</span>");
                    sb.Append(body.Substring(firstCharIndex + 1));
                }
                else sb.Append(body);
                result = await _formatter.CreateHtml(item.FormattedForMainPageSourceAndDate, item.Title, item.Link, sb.ToString(), "#333333", "#FFFFFF", "Segoe UI", "12", "#0000FF", item.ImageUrl);
            }
            catch (Exception e)
            {
                App.LogError("Error getting mobilized html", e);
                result = null;
            }

            return result;
        }

        private static int FindFirstCharIndex(String s)
        {
            int found = -1;
            int tagLevel = 0;
            int index = 0;
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsLetter(c) && tagLevel == 0)
                {
                    found = index;
                    break;
                }
                else if (c == '<') tagLevel++;
                else if (c == '>') tagLevel--;
                index++;
            }

            return found;
        }

    } // end of class
}
