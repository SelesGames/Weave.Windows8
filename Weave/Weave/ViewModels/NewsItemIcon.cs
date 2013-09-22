using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels
{
    public class NewsItemIcon : NewsItem
    {
        public string SourceIconUrl { get; set; }

        public NewsItemIcon(NewsItem item)
        {
            this.Feed = item.Feed;
            this.HasBeenViewed = item.HasBeenViewed;
            this.Id = item.Id;
            this.Image = item.Image;
            this.ImageUrl = item.ImageUrl;
            this.IsFavorite = item.IsFavorite;
            this.IsNew = item.IsNew;
            this.Link = item.Link;
            this.OriginalDownloadDateTime = item.OriginalDownloadDateTime;
            this.PodcastUri = item.PodcastUri;
            this.SourceIconUrl = String.IsNullOrEmpty(item.Feed.IconUrl) ? Weave.Common.SourceIconHelper.GetIcon(item.OriginalFeedUri) : item.Feed.IconUrl;
            this.Title = item.Title;
            this.UtcPublishDateTime = item.UtcPublishDateTime;
            this.VideoUri = item.VideoUri;
            this.YoutubeId = item.YoutubeId;
            this.ZuneAppId = item.ZuneAppId;
        }

    } // end of class
}
