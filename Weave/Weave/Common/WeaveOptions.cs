using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Weave.Common
{
    public static class WeaveOptions
    {
        public enum FontSize { Small = 14, Medium = 18, Large = 23 };

        private const String FontSizeKey = "FontSize";
        private static FontSize? _fontSize = null;

        public static FontSize CurrentFontSize
        {
            get
            {
                if (_fontSize == null)
                {
                    ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                    FontSize fontSize;
                    if (container.Values.ContainsKey(FontSizeKey) && Enum.TryParse<FontSize>((String)container.Values[FontSizeKey], out fontSize)) _fontSize = fontSize;
                    else _fontSize = FontSize.Medium;
                }
                return _fontSize == null ? FontSize.Medium : _fontSize.Value;
            }
            set
            {
                _fontSize = value;
                ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                container.Values[FontSizeKey] = value.ToString();
            }
        }

        public enum LayoutSize { Normal, Large };

        private const String LayoutSizeKey = "LayoutSize";
        private static LayoutSize? _layoutSize = null;
        public static LayoutSize CurrentLayoutSize
        {
            get
            {
                if (_layoutSize == null)
                {
                    ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                    LayoutSize layoutSize;
                    if (container.Values.ContainsKey(LayoutSizeKey) && Enum.TryParse<LayoutSize>((String)container.Values[LayoutSizeKey], out layoutSize)) _layoutSize = layoutSize;
                    else _layoutSize = LayoutSize.Normal;
                }
                return _layoutSize == null ? LayoutSize.Normal : _layoutSize.Value;
            }
            set
            {
                _layoutSize = value;
                ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                container.Values[LayoutSizeKey] = value.ToString();
            }
        }

        public enum ReadingTheme { Light, Dark };
        private const String ReadingThemeKey = "ReadingTheme";
        private static ReadingTheme? _readingTheme = null;
        public static ReadingTheme CurrentReadingTheme
        {
            get
            {
                if (_readingTheme == null)
                {
                    ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                    ReadingTheme readingTheme;
                    if (container.Values.ContainsKey(ReadingThemeKey) && Enum.TryParse<ReadingTheme>((String)container.Values[ReadingThemeKey], out readingTheme)) _readingTheme = readingTheme;
                    else _readingTheme = ReadingTheme.Light;
                }
                return _readingTheme == null ? ReadingTheme.Light : _readingTheme.Value;
            }
            set
            {
                _readingTheme = value;
                ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                container.Values[ReadingThemeKey] = value.ToString();
            }
        }

        public enum ArticlePlacement { Right, Center };
        private const String ArticlePlacementKey = "ArticlePlacement";
        private static ArticlePlacement? _articlePlacement = null;
        public static ArticlePlacement CurrentArticlePlacement
        {
            get
            {
                if (_articlePlacement == null)
                {
                    ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                    ArticlePlacement placement;
                    if (container.Values.ContainsKey(ArticlePlacementKey) && Enum.TryParse<ArticlePlacement>((String)container.Values[ArticlePlacementKey], out placement)) _articlePlacement = placement;
                    else _articlePlacement = CheckForMouse() ? ArticlePlacement.Center : ArticlePlacement.Right;
                }
                return _articlePlacement == null ? ArticlePlacement.Right : _articlePlacement.Value;
            }
            set
            {
                _articlePlacement = value;
                ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
                container.Values[ArticlePlacementKey] = value.ToString();
            }
        }

        private static bool CheckForMouse()
        {
            bool hasMouse;
            Windows.Devices.Input.MouseCapabilities mouseCapabilities = new Windows.Devices.Input.MouseCapabilities();
            Windows.Devices.Input.TouchCapabilities touchCapabilities = new Windows.Devices.Input.TouchCapabilities();
            if (touchCapabilities.TouchPresent == 0) hasMouse = true;
            else hasMouse = mouseCapabilities.MousePresent > 0;
            return hasMouse;
        }

    }
}
