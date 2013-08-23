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
        public enum FontSize { Small = 12, Medium = 16, Large = 20 };

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

    }
}
