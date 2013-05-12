using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartHeroArticle : BindableBase
    {
        private NewsItem _article;

        public NewsItem Article
        {
            get { return _article; }
            set { SetProperty(ref _article, value); }
        }

    } // end of class
}
