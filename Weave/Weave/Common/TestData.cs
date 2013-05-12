using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.StartHub;

namespace Weave.Common
{
    public static class TestData
    {
        public static NewsItem GetHeroItemSample()
        {
            NewsItem item = new NewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Microsoft and others file EU antitrust complaint over Android app bundling";
            item.Feed = new Feed() { Name = "The Verge" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 1:59pm";
            return item;
        }

        public static List<StartNewsItem> GetLatestArticlesSample()
        {
            List<StartNewsItem> items = new List<StartNewsItem>();
            StartNewsItem item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Fifteen days in Rome: how the pope was picked";
            item.Feed = new Feed() { Name = "Wall Street Journel" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.IsMain = true;
            item.WidthSpan = 2;
            item.HeightSpan = 3;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Margret Thatcher and her influence on women";
            item.Feed = new Feed() { Name = "The Guardian" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Next Xbox rumors, specs, developers,";
            item.Feed = new Feed() { Name = "The Verge" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 2;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "U.S.-China Nuclear Silence Leaves Void";
            item.Feed = new Feed() { Name = "The Verge" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 2;
            item.ShowImage = true;
            items.Add(item);

            return items;
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

        public static List<StartNewsItem> GetWorldNewsSample()
        {
            List<StartNewsItem> items = new List<StartNewsItem>();
            StartNewsItem item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "North Korea risks further isolation with missile launch";
            item.Feed = new Feed() { Name = "The New York Times" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.IsMain = true;
            item.WidthSpan = 2;
            item.HeightSpan = 2;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Mohamed Morsi backs Egyptian military after malpractice";
            item.Feed = new Feed() { Name = "The Associated Press" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Aftermath of plane crashing into sea in Bali";
            item.Feed = new Feed() { Name = "Wall Street Journel" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "US risks wrath of Moscow with threat to offcials on 'Magnitsky list'";
            item.Feed = new Feed() { Name = "The Guardian" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            return items;
        }

        public static List<StartNewsItem> GetBusinessSample()
        {
            List<StartNewsItem> items = new List<StartNewsItem>();
            StartNewsItem item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Portugal and Ireland to be given more bailout repayment time";
            item.Feed = new Feed() { Name = "BBC News" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:33pm";
            item.IsMain = true;
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Court ruling goes against RBS";
            item.Feed = new Feed() { Name = "The Guardian" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 1:00pm";
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Obama unveils $3.8tn budget plan";
            item.Feed = new Feed() { Name = "BI" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "How one company taught its employees how to be happier";
            item.Feed = new Feed() { Name = "Fast Company" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 3:22pm";
            item.WidthSpan = 1;
            item.HeightSpan = 1;
            item.ShowImage = true;
            items.Add(item);

            item = new StartNewsItem();
            item.Id = Guid.NewGuid();
            item.Title = "Australian $40bn LNG project shelved";
            item.Feed = new Feed() { Name = "Forbes" };
            item.UtcPublishDateTime = "Tuesday 12 April 2013, 12:22pm";
            item.WidthSpan = 2;
            item.HeightSpan = 1;
            item.ShowImage = false;
            items.Add(item);

            return items;
        }

    } // end of class
}
