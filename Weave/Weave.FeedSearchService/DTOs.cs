using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Weave.FeedSearchService
{
    public class FeedApiResult
    {
        public ResponseData responseData { get; set; }
        public string responseStatus { get; set; }
    }

    public class ResponseData
    {
        public List<Entry> entries { get; set; }
    }

    public class Entry
    {
        public string url { get; set; }
        public string title { get; set; }
        public string contentSnippet { get; set; }

        public void Sanitize()
        {
            title = Uri.UnescapeDataString(HtmlSanitizer.Sanitize(title));
        }
    }
}
