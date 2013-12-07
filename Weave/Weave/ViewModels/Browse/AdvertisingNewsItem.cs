using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels.Browse
{
    public class AdvertisingNewsItem : NewsItem
    {
        public enum FallbackSelect { Weave };

        private const String WeaveImage = "/Assets/Ads/250x250-WeaveWP.png";
        private const String WeaveImageWide = "/Assets/Ads/440x380-WeaveWP.png";

        private FallbackSelect _currentFallbackAd = FallbackSelect.Weave;

        private bool _showFallbackAd = false;
        public bool ShowFallbackAd
        {
            get { return _showFallbackAd; }
            private set
            {
                if (_showFallbackAd != value)
                {
                    _showFallbackAd = value;
                    Raise("ShowFallbackAd");
                }
            }
        }

        private String _fallbackAdPath = WeaveImage;
        public String FallbackAdPath
        {
            get { return _fallbackAdPath; }
            private set
            {
                if (!String.Equals(_fallbackAdPath, value))
                {
                    _fallbackAdPath = value;
                    Raise("FallbackAdPath");
                }
            }
        }

        public void SelectFallbackAd(bool wide)
        {
            FallbackAdPath = wide ? WeaveImageWide : WeaveImage;
            ShowFallbackAd = true;
        }

        public void ExecuteFallbackAd()
        {
            switch (_currentFallbackAd)
            {
                case FallbackSelect.Weave:
                    Windows.System.Launcher.LaunchUriAsync(new Uri(StartHub.StartAdvertisingViewModel.WeaveWpUrl));
                    break;
                default:
                    break;
            }
        }
    }
}
