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
                    else fontSize = FontSize.Medium;
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
                    else layoutSize = LayoutSize.Normal;
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

    }
}
