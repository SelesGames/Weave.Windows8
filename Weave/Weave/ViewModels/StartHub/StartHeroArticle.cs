using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartHeroArticle : StartItemBase
    {
        private NewsItem _article;

        public NewsItem Article
        {
            get { return _article; }
            set { SetProperty(ref _article, value); }
        }

        public override void OnItemClick(object item)
        {
            Dictionary<String, object> parameters = new Dictionary<string, object>();
            if (item is StartHeroArticle)
            {
                parameters[BrowsePage.NavParamSelectionKey] = ((StartHeroArticle)item).Article.Id;
            }
            App.Navigate(typeof(BrowsePage), parameters);
        }

    } // end of class
}
