using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Weave.Common
{
    public static class LiveTileHelper
    {
        public const String LiveTileSettingsContainer = "LiveTiles";
        public const String MainTileLatestIdKey = "MainTileLatestId";

        private static ApplicationDataContainer GetLiveTileContainer()
        {
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings.CreateContainer(LiveTileSettingsContainer, ApplicationDataCreateDisposition.Always);
            return container;
        }

        public static bool RequireMainTileUpdate(String latestId)
        {
            ApplicationDataContainer container = GetLiveTileContainer();
            String storedId = null;
            if (container.Values.ContainsKey(MainTileLatestIdKey)) storedId = (String)container.Values[MainTileLatestIdKey];
            return !String.Equals(latestId, storedId);
        }

        public static void UpdateMainTileLatestId(String latestId)
        {
            ApplicationDataContainer container = GetLiveTileContainer();
            container.Values[MainTileLatestIdKey] = latestId;
        }

        private static void ClearTileLatestId()
        {
            ApplicationDataContainer container = GetLiveTileContainer();
            if (container.Values.ContainsKey(MainTileLatestIdKey)) container.Values.Remove(MainTileLatestIdKey);
        }

        public static void ResetMainTile()
        {
            ClearMainTile();
            ClearTileLatestId();
        }

        public static void ClearMainTile()
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch (Exception e)
            {
                App.LogError("Error clearing main tile", e);
            }
        }

        public static void EnableTileQueue(bool enable)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(enable);
            }
            catch (Exception)
            {
            }
        }

        public static void UpdateVideoTile(NewsItem item)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImageAndText01);

            XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
            ((XmlElement)tileImageAttributes[0]).SetAttribute("src", item.ImageUrl);
            ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "Article image");
            //if (item.Feed != null && !String.IsNullOrEmpty(item.Feed.IconUrl))
            //{
            //    ((XmlElement)tileImageAttributes[1]).SetAttribute("src", item.Feed.IconUrl);
            //    ((XmlElement)tileImageAttributes[1]).SetAttribute("alt", "Source image");
            //}
            tileXml.GetElementsByTagName("text")[0].InnerText = item.Title;

            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.Tag = item.Id.GetHashCode().ToString();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            //else TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId).Update(tileNotification);
        }

    } // end of class
}
