using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;
using Weave.ViewModels;

namespace Weave.ViewModels.StartHub
{
    public class StartNewsItemContainer : IResizable
    {
        private NewsItem _newsItem;
        public NewsItem NewsItem
        {
            get { return _newsItem; }
            set { _newsItem = value; }
        }

        public StartNewsItemContainer()
        {
        }

        public StartNewsItemContainer(NewsItem item)
        {
            NewsItem = item;
        }

        private int _widthSpan = 1;
        /// <summary>
        /// Gets or sets the width span of this video (used with variable size grid view).
        /// </summary>
        public int WidthSpan
        {
            get { return _widthSpan; }
            set { _widthSpan = value; }
        }

        private int _heightSpan = 1;
        /// <summary>
        /// Gets or sets the width span of this video (used with variable size grid view).
        /// </summary>
        public int HeightSpan
        {
            get { return _heightSpan; }
            set { _heightSpan = value; }
        }

        private bool _showImage = true;
        /// <summary>
        /// Indicates if the image should be shown for this item.
        /// </summary>
        public bool ShowImage
        {
            get { return _showImage; }
            set { _showImage = value; }
        }

        private bool _isMain;
        /// <summary>
        /// Indicates if this article is the main article in its section.
        /// </summary>
        public bool IsMain
        {
            get { return _isMain; }
            set { _isMain = value; }
        }

    } // end of class
}
