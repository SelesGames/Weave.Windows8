﻿using System;
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

        public Feed Feed { get; set; }
        public CategoryViewModel ParentCategory { get; set; }

        public FeedItemViewModel(Feed feed)
        {
            this.Feed = feed;
            if (feed != null)
            {
                this.NewCount = feed.NewArticleCount;
                this.IconUrl = Weave.Common.SourceIconHelper.GetIcon(feed.Uri);
            }
        }

        private bool _requiresRefresh;
        public bool RequiresRefresh
        {
            get { return _requiresRefresh; }
            set { SetProperty(ref _requiresRefresh, value); }
        }
    }
}