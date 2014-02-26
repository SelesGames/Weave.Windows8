
namespace System.Text.RegularExpressions
{
    internal static class HtmlSanitizer
    {
        static Regex _tags = new Regex("<[^>]*(>|$)", RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            string tagname;
            Match tag;

            // match every HTML tag in the input
            MatchCollection tags = _tags.Matches(html);
            for (int i = tags.Count - 1; i > -1; i--)
            {
                tag = tags[i];
                tagname = tag.Value.ToLowerInvariant();

                html = html.Remove(tag.Index, tag.Length);
            }

            return html;
        }
    }
}
