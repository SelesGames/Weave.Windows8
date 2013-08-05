using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Browse;
using Weave.ViewModels.StartHub;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Weave.Common
{
    public class StartItemSelector : DataTemplateSelector
    {
        public DataTemplate HeroTemplate { get; set; }
        public DataTemplate LatestTemplate { get; set; }
        public DataTemplate SourcesTemplate { get; set; }
        public DataTemplate ClusterTemplate { get; set; }
        public DataTemplate AddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item is StartLatestViewModel) return LatestTemplate;
                else if (item is StartClusterViewModel) return ClusterTemplate;
                else if (item is StartSourcesViewModel) return SourcesTemplate;
                else if (item is StartAddViewModel) return AddTemplate;
                else if (item is StartHeroArticle) return HeroTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public class StartLatestItemSelector : DataTemplateSelector
    {
        public DataTemplate MainTemplate { get; set; }
        public DataTemplate MainNoImageTemplate { get; set; }
        public DataTemplate LargeTemplate { get; set; }
        public DataTemplate LargeNoImageTemplate { get; set; }
        public DataTemplate SmallTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item is StartNewsItemContainer)
            {
                StartNewsItemContainer newsItem = (StartNewsItemContainer)item;
                if (newsItem.WidthSpan == 2)
                {
                    if (newsItem.IsMain)
                    {
                        if (newsItem.ShowImage) return MainTemplate;
                        else return MainNoImageTemplate;
                    }
                    else
                    {
                        if (newsItem.ShowImage) return LargeTemplate;
                        else return LargeNoImageTemplate;
                    }
                }
                else return SmallTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public class StartClusterItemSelector : DataTemplateSelector
    {
        public DataTemplate MainTemplate { get; set; }
        public DataTemplate MainNoImageTemplate { get; set; }
        public DataTemplate LargeTemplate { get; set; }
        public DataTemplate LargeNoImageTemplate { get; set; }
        public DataTemplate SmallTemplate { get; set; }
        public DataTemplate SmallNoImageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item is StartNewsItemContainer)
            {
                StartNewsItemContainer newsItem = (StartNewsItemContainer)item;
                if (newsItem.WidthSpan == 2)
                {
                    if (newsItem.IsMain)
                    {
                        if (newsItem.ShowImage) return MainTemplate;
                        else return MainNoImageTemplate;
                    }
                    else
                    {
                        return LargeNoImageTemplate;
                    }
                }
                else
                {
                    if (newsItem.ShowImage && newsItem.NewsItem.HasImage) return SmallTemplate;
                    else return SmallNoImageTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public class NavigationItemSelector : DataTemplateSelector
    {
        public DataTemplate FeedTemplate { get; set; }
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate SpacerTemplate { get; set; }
        public DataTemplate AddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is FeedItemViewModel) return FeedTemplate;
                else if (item is CategoryViewModel) return CategoryTemplate;
                else if (item is SpacerViewModel) return SpacerTemplate;
                else if (item is FeedManagementViewModel) return AddTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public class BrowseArticleSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null && item is NewsItem)
            {
                NewsItem article = (NewsItem)item;
                if (article.HasImage) return ImageTemplate;
                else return TextTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
