using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels
{
    public class FeedWithIcon : Feed
    {
        public String IconUrl { get; set; }

        public FeedWithIcon(Feed feed)
        {
            this.ArticleViewingType = feed.ArticleViewingType;
            this.Category = feed.Category;
            this.IconUrl = Weave.Common.SourceIconHelper.GetIcon(feed.Uri);
            this.Id = feed.Id;
            this.Name = feed.Name;
            this.NewArticleCount = feed.NewArticleCount;
            this.TeaserImageUrl = feed.TeaserImageUrl;
            this.TotalArticleCount = feed.TotalArticleCount;
            this.UnreadArticleCount = feed.UnreadArticleCount;
            this.Uri = feed.Uri;
        }

    } // end of class
}
