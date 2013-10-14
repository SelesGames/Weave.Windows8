using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;
using Weave.ViewModels.Browse;

namespace Weave.ViewModels
{
    public class FeedItemViewModel : BindableBase
    {
        private bool _isAdded;
        public bool IsAdded
        {
            get { return _isAdded; }
            set { SetProperty(ref _isAdded, value); }
        }

        private int _newCount;
        public int NewCount
        {
            get { return _newCount; }
            set
            {
                SetProperty(ref _newCount, value);
                if (Feed != null) Feed.NewArticleCount = value;
            }
        }

        private String _iconUrl;
        public String IconUrl
        {
            get { return _iconUrl; }
            set { SetProperty(ref _iconUrl, value); }
        }

        private Feed _feed;
        public Feed Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }
        public CategoryViewModel ParentCategory { get; set; }

        public FeedItemViewModel(Feed feed)
        {
            this.Feed = feed;
            if (feed != null)
            {
                this.NewCount = feed.NewArticleCount;
                this.IconUrl = String.IsNullOrEmpty(feed.IconUrl) ? Weave.Common.SourceIconHelper.GetIcon(feed.Uri) : feed.IconUrl;
            }
        }

        private bool _requiresRefresh;
        public bool RequiresRefresh
        {
            get { return _requiresRefresh; }
            set { SetProperty(ref _requiresRefresh, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
    }
}
