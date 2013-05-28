using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Browse;
using Weave.ViewModels.StartHub;

namespace Weave.Common
{
    public static class TestData
    {
        public static List<object> GetNavigationSample()
        {
            List<Object> navItems = new List<Object>();

            navItems.Add(new CategoryViewModel() { DisplayName = "All News", NewCount = 143 });

            navItems.Add(new SpacerViewModel() { Height = 20 });

            navItems.Add(new CategoryViewModel() { DisplayName = "Business", Info = new CategoryInfo() { Category = "Business" }, NewCount = 22 });
            navItems.Add(new Feed() { Name = "Forbes" });
            navItems.Add(new Feed() { Name = "Business Insider" });
            navItems.Add(new Feed() { Name = "The Guardian" });

            navItems.Add(new SpacerViewModel() { Height = 20 });

            navItems.Add(new CategoryViewModel() { DisplayName = "World News", Info = new CategoryInfo() { Category = "World News" }, NewCount = 121 });
            navItems.Add(new Feed() { Name = "New York Times" });
            navItems.Add(new Feed() { Name = "Associated Press" });
            navItems.Add(new Feed() { Name = "Wall Street Journal" });

            navItems.Add(new SpacerViewModel() { Height = 20 });

            return navItems;
        }

        public static NewsItem GetHeroItemSample()
        {
            NewsItem item = new NewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Microsoft and others file EU antitrust complaint over Android app bundling";
            item.Feed = new Feed() { Name = "The Verge" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 1:59pm";
            return item;
        }

        public static List<String> GetSampleSources()
        {
            List<String> sources = new List<string>()
            {
                "art + design",
                "business",
                "microsoft",
                "technology",
                "windows phone",
                "world news",
                "sports",
                "all sources"
            };
            return sources;
        }

        public static List<NewsItem> GetNewsFeedSample(int count)
        {
            List<NewsItem> items = new List<NewsItem>();
            NewsItem newsItem;
            for (int i = 0; i < count; i++)
            {
                newsItem = new NewsItem();
                newsItem.Id = Guid.NewGuid();
                newsItem.Title = "How one company taught its employees how to be happier";
                newsItem.Feed = new Feed() { Name = "Fast Company" };
                newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
                items.Add(newsItem);
            }
            return items;
        }

        public static List<StartNewsItemContainer> GetWorldNewsSample()
        {
            List<StartNewsItemContainer> items = new List<StartNewsItemContainer>();
            StartNewsItemContainer item = new StartNewsItemContainer();
            NewsItem newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "North Korea risks further isolation with missile launch";
            newsItem.Feed = new Feed() { Name = "The New York Times" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.IsMain = true;
            item.WidthSpan = 2;
            item.HeightSpan = 2;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Mohamed Morsi backs Egyptian military after malpractice";
            newsItem.Feed = new Feed() { Name = "The Associated Press" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Aftermath of plane crashing into sea in Bali";
            newsItem.Feed = new Feed() { Name = "Wall Street Journel" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "US risks wrath of Moscow with threat to offcials on 'Magnitsky list'";
            newsItem.Feed = new Feed() { Name = "The Guardian" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            return items;
        }

        public static List<StartNewsItemContainer> GetBusinessSample()
        {
            List<StartNewsItemContainer> items = new List<StartNewsItemContainer>();
            StartNewsItemContainer item = new StartNewsItemContainer();
            NewsItem newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Portugal and Ireland to be given more bailout repayment time";
            newsItem.Feed = new Feed() { Name = "BBC News" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:33pm";
            item.NewsItem = newsItem;
            item.IsMain = true;
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Court ruling goes against RBS";
            newsItem.Feed = new Feed() { Name = "The Guardian" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 1:00pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Obama unveils $3.8tn budget plan";
            newsItem.Feed = new Feed() { Name = "BI" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "How one company taught its employees how to be happier";
            newsItem.Feed = new Feed() { Name = "Fast Company" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItemContainer();
            newsItem = new NewsItem();
            newsItem.Id = Guid.NewGuid();
            newsItem.Title = "Australian $40bn LNG project shelved";
            newsItem.Feed = new Feed() { Name = "Forbes" };
            newsItem.UtcPublishDateTime = "Tuesday 12 April 2013, 12:22pm";
            item.NewsItem = newsItem;
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            return items;
        }

    } // end of class
}
