using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.ViewModels;
using Windows.UI.Xaml.Media;

namespace Weave.Common
{
    public static class MobilizerHelper
    {
        private static Formatter _formatter = new Formatter();
        private static Client _client = new Client();

        private static HashSet<String> _ignoreParagraphClasses = new HashSet<string>()
        {
            "article-date",
            "photo_credit",
        };

        public static Brush DarkBackgroundBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 18, 18, 18));
        public static Brush LightBackgroundBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 242, 242, 242));

        public static String GetForeground()
        {
            if (WeaveOptions.CurrentReadingTheme == WeaveOptions.ReadingTheme.Light) return "#101010";
            //if (WeaveOptions.CurrentReadingTheme == WeaveOptions.ReadingTheme.Light) return "#333333";
            else return "#FAFAFA";
        }

        public static String GetBackground()
        {
            if (WeaveOptions.CurrentReadingTheme == WeaveOptions.ReadingTheme.Light) return "#F2F2F2";
            else return "#121212";
        }

        public static async Task<String> GetMobilizedBody(NewsItem item)
        {
            String result = null;
            try
            {
                result = (await _client.Get(item.Link)).content;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static async Task<String> GetMobilizedHtml(NewsItem item, int fontSize, int articleWidth)
        {
            String result = null;

            try
            {
                var content = await _client.Get(item.Link);
                String body = content.content;
                int firstCharIndex = -1;
                StringBuilder sb = new StringBuilder();
                String paragraphPattern = "<p(?<properties>[^>]*)>(?<content>.*)</p>";

                foreach (Match m in Regex.Matches(body, paragraphPattern, RegexOptions.Singleline))
                {
                    if (m.Success && m.Groups["content"].Success)
                    {
                        Group g = m.Groups["content"];
                        //String properties = m.Groups["properties"].Value;
                        //if (!String.IsNullOrEmpty(properties))
                        //{
                        //    Match classMatch = Regex.Match(properties, "class=\"(?<class>[^\"]*)\"");
                        //    if (classMatch.Success && _ignoreParagraphClasses.Contains(classMatch.Groups["class"].Value))
                        //    {
                        //        int newStartIndex = g.Index;
                        //    }
                        //}

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

                String sourceIcon = null;
                String imageUrl = null;
                if (item.HasImage)
                {
                    if (item.Image != null) imageUrl = item.Image.OriginalUrl;
                    else if (!String.IsNullOrEmpty(item.ImageUrl)) imageUrl = item.ImageUrl;
                }
                else
                {
                    if (item.Feed != null && !String.IsNullOrEmpty(item.Feed.IconUrl)) sourceIcon = item.Feed.IconUrl;
                    else sourceIcon = SourceIconHelper.GetWebIcon(item.Feed.Uri);
                }

                result = await _formatter.CreateHtml(item.FormattedForMainPageSourceAndDate.Replace('•', '|'), item.Title, item.Link, sb.ToString(), GetForeground(), GetBackground(), "Cambria", fontSize + "pt", "#E96113", imageUrl, sourceIcon, articleWidth);
            }
            catch (Exception e)
            {
                App.LogError("Error getting mobilized html", e);
                return null;
            }

            return result;
        }

        private static int FindFirstCharIndex(String s)
        {
            int found = -1;
            int tagLevel = 0;
            int index = 0;
            bool insideSpecialChar = false;
            foreach (Char c in s.ToCharArray())
            {
                if (Char.IsLetter(c) && tagLevel == 0 && !insideSpecialChar)
                {
                    found = index;
                    break;
                }
                else
                {
                    switch (c)
                    {
                        case '<':
                            tagLevel++;
                            break;
                        case '>':
                            tagLevel--;
                            break;
                        case '&':
                            if (!insideSpecialChar) insideSpecialChar = true;
                            break;
                        case ';':
                            if (insideSpecialChar) insideSpecialChar = false;
                            break;
                    }
                }
                index++;
            }

            return found;
        }

    } // end of class
}
